using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public class DocumentDiagnosticService : IDiagnosticService
{
    private readonly ConcurrentDictionary<DocumentUri, Dictionary<string, List<Diagnostic>>> _diagnostics = new();

    private readonly Action<DocumentUri> _onDiagnosticChanged;

    public DocumentDiagnosticService()
    {
        _onDiagnosticChanged = ((Action<DocumentUri>)OnDiagnosticChanged).Debounce(TimeSpan.FromMilliseconds(500));
    }

    private void OnDiagnosticChanged(DocumentUri uri)
    {
        DataUpdated?.Invoke(this, new(uri, GetData(uri) ?? new()));
    }

    public void SetDiagnostics(DocumentUri uri, string provider, IEnumerable<Diagnostic> diagnostics)
    {
        var enumerable = diagnostics.ToList();
        _diagnostics.AddOrUpdate
        (
            uri,
            new Dictionary<string, List<Diagnostic>> { { provider, enumerable } },
            (_, dictionary) =>
            {
                dictionary[provider] = enumerable;
                return dictionary;
            }
        );

        _onDiagnosticChanged(uri);
    }

    #region IDiagnosticService Implementation

    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken)
    {
        if (!_diagnostics.TryGetValue(uri, out var diagnostics))
            return Enumerable.Empty<Diagnostic>();

        return diagnostics.Values.SelectMany(x => x);
    }

    #endregion

    #region IProviderService<Container<Diagnostic>> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<Container<Diagnostic>>>? DataUpdated;

    public Container<Diagnostic>? GetData(DocumentUri uri)
    {
        return _diagnostics.TryGetValue(uri, out var diagnostics) ? new(diagnostics.Values.SelectMany(x => x)) : null;
    }

    public void SetData(DocumentUri uri, Container<Diagnostic> data, bool notify = true)
    {
        throw new InvalidOperationException("DocumentDiagnosticService does not support SetData");
    }

    #endregion
}
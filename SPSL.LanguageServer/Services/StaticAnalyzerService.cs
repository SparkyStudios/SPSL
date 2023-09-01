using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Symbols;
using SPSL.LanguageServer.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Services;

public class StaticAnalyzerService : IDiagnosticService
{
    private const string SourceName = "spsl";
    private const string MismatchConstructorNameErrorCode = "0001";
    private const string ConflictingIdentifierNameErrorCode = "0002";
    private const string ReassignedConstVariableErrorCode = "0003";

    private readonly ConcurrentDictionary<DocumentUri, List<Diagnostic>> _cache = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly ConfigurationService _configurationService;
    private readonly SymbolProviderService _symbolProviderService;

    public StaticAnalyzerService
    (
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        SymbolProviderService symbolProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _symbolProviderService = symbolProviderService;

        _symbolProviderService.DataUpdated += SymbolProviderServiceOnDataUpdated;
    }

    private void SymbolProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<SymbolTable> e)
    {
        Document document = _documentManagerService.GetDocument(e.Uri);

        var diagnostics = _cache.GetOrAdd(e.Uri, new List<Diagnostic>());
        diagnostics.Clear();

        CheckShaderConstructorNames(document, e.Data, diagnostics);
        CheckConflictingVariables(document, e.Data, diagnostics);

        DataUpdated?.Invoke(document, new(e.Uri, diagnostics));
    }

    private void CheckShaderConstructorNames(Document document, SymbolTable table, ICollection<Diagnostic> diagnostics)
    {
        if (table.Type == SymbolType.Shader)
        {
            Symbol? constructor = table.Symbols.FirstOrDefault(symbol => symbol.Type == SymbolType.Constructor);
            if (constructor == null)
                return;

            if (constructor.Name == table.Name) return;

            Range range = new()
            {
                Start = document.PositionAt(constructor.Start),
                End = document.PositionAt(constructor.End + 1)
            };

            diagnostics.Add
            (
                new()
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = range,
                    Message = "Mismatching constructor name.",
                    Source = SourceName,
                    Code = MismatchConstructorNameErrorCode,
                    RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                        ? new
                        (
                            new DiagnosticRelatedInformation
                            {
                                Message = $"The constructor doesn't match the shader name: {table.Name}",
                                Location = new()
                                {
                                    Uri = document.Uri,
                                    Range = range
                                }
                            }
                        )
                        : new Container<DiagnosticRelatedInformation>()
                }
            );
        }
        else
        {
            foreach (Symbol symbol in table.Symbols)
                if (symbol is SymbolTable t)
                    CheckShaderConstructorNames(document, t, diagnostics);
        }
    }

    private void CheckConflictingVariables(Document document, SymbolTable table, ICollection<Diagnostic> diagnostics)
    {
        table.Symbols.Where(symbol => symbol.Key.Contains('@')).ToList().ForEach(symbol =>
        {
            Range range = new()
            {
                Start = document.PositionAt(symbol.Start),
                End = document.PositionAt(symbol.End + 1)
            };

            diagnostics.Add
            (
                new()
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = range,
                    Message = symbol.Type switch {
                        SymbolType.Function => "A function with this signature already exists.",
                        _ => "Conflicting identifier."
                    },
                    Source = SourceName,
                    Code = ConflictingIdentifierNameErrorCode,
                    RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                        ? new
                        (
                            new DiagnosticRelatedInformation
                            {
                                Message = symbol.Type switch {
                                    SymbolType.Function => "You cannot declare two function with the exact same signature, on only with different return values.",
                                    _ => "The name given to this identifier is already used."
                                },
                                Location = new()
                                {
                                    Uri = document.Uri,
                                    Range = range
                                }
                            }
                        )
                        : new Container<DiagnosticRelatedInformation>()
                }
            );
        });

        foreach (Symbol symbol in table.Symbols)
            if (symbol is SymbolTable t)
                CheckConflictingVariables(document, t, diagnostics);
    }

    #region IDiagnosticService Implementation

    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken)
    {
        if (!_documentManagerService.HasDocument(uri))
            return ArraySegment<Diagnostic>.Empty;

        return _cache.TryGetValue(uri, out var diagnostics) ? diagnostics : ArraySegment<Diagnostic>.Empty;
    }

    #endregion

    #region IProviderService<Container<Diagnostic>> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<Container<Diagnostic>>>? DataUpdated;

    public Container<Diagnostic>? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out var diagnostics) ? new(diagnostics) : null;
    }

    public void SetData(DocumentUri uri, Container<Diagnostic> data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data.ToList(), (k, v) => data.ToList());

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Services;

public class SyntaxAnalyzerService : IDiagnosticService
{
    private class SyntaxErrorListener : BaseErrorListener
    {
        private const string SourceName = "spsl";
        private const string DiagnosticId = "syntax-error";

        private readonly ConfigurationService _configurationService;
        private readonly List<Diagnostic> _diagnostics;
        private readonly Document _document;

        public Container<Diagnostic> Diagnostics => _diagnostics;

        public SyntaxErrorListener
        (
            Document document,
            List<Diagnostic> diagnostics,
            ConfigurationService configurationService
        )
        {
            _configurationService = configurationService;
            _document = document;
            _diagnostics = diagnostics;
        }

        public override void SyntaxError
        (
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line,
            int charPositionInLine,
            string msg,
            RecognitionException e
        )
        {
            Range range = new()
            {
                Start = _document.PositionAt(offendingSymbol.StartIndex),
                End = _document.PositionAt(offendingSymbol.StopIndex + 1)
            };

            Diagnostic diagnostic = new()
            {
                Severity = DiagnosticSeverity.Error,
                Range = range,
                Message = msg,
                Source = SourceName,
                Code = DiagnosticId,
                RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                    ? new
                    (
                        new DiagnosticRelatedInformation
                        {
                            Message = $"Syntax Error: {msg}",
                            Location = new()
                            {
                                Uri = _document.Uri,
                                Range = range
                            }
                        }
                    )
                    : new Container<DiagnosticRelatedInformation>()
            };

            _diagnostics.Add(diagnostic);
        }
    }

    private readonly ConcurrentDictionary<DocumentUri, List<Diagnostic>> _cache = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly ConfigurationService _configurationService;
    private readonly TokenProviderService _tokenProviderService;

    public SyntaxAnalyzerService
    (
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        TokenProviderService tokenProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.CollectParserErrorListeners += OnCollectParserErrorListeners;
        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void OnCollectParserErrorListeners(object? sender, CollectParserErrorListenersEventArgs e)
    {
        if (!_documentManagerService.HasDocument(e.Uri))
            return;

        Document document = _documentManagerService.GetData(e.Uri);

        var diagnostics = _cache.GetOrAdd(e.Uri, new List<Diagnostic>());
        diagnostics.Clear();

        SyntaxErrorListener listener = new
        (
            document,
            diagnostics,
            _configurationService
        );

        e.ErrorListeners.Add(listener);
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        DataUpdated?.Invoke
        (
            this,
            new(e.Uri, GetData(e.Uri) ?? new())
        );
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
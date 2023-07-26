using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Services;

public class SyntaxDiagnosticService : IDiagnosticService
{
    private class SyntaxErrorListener : BaseErrorListener
    {
        private const string SourceName = "spsl-syntax-analyzer";

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
                RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                    ? new Container<DiagnosticRelatedInformation>
                    (
                        new DiagnosticRelatedInformation
                        {
                            Message = $"Syntax Error: {msg}",
                            Location = new Location
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

    private readonly ConcurrentDictionary<DocumentUri, List<Diagnostic>> _diagnostics = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly ConfigurationService _configurationService;
    private readonly AstProviderService _astProviderService;

    public event EventHandler<DiagnosticReadyEventArgs>? DiagnosticReady;

    public SyntaxDiagnosticService
    (
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        AstProviderService astProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _astProviderService = astProviderService;

        _astProviderService.CollectParserErrorListeners += OnCollectParserErrorListeners;
        _astProviderService.DocumentAstChanged += OnDocumentAstChanged;
    }

    private void OnCollectParserErrorListeners(object? sender, CollectParserErrorListenersEventArgs e)
    {
        if (!_documentManagerService.HasDocument(e.Uri))
            return;

        Document document = _documentManagerService.GetDocument(e.Uri);

        var diagnostics = _diagnostics.GetOrAdd(e.Uri, new List<Diagnostic>());
        diagnostics.Clear();

        SyntaxErrorListener listener = new
        (
            document,
            diagnostics,
            _configurationService
        );

        e.ErrorListeners.Add(listener);
    }

    private void OnDocumentAstChanged(object? sender, DocumentAstEventArgs e)
    {
        DiagnosticReady?.Invoke
        (
            this,
            new(e.Uri, new(_diagnostics.GetOrAdd(e.Uri, new List<Diagnostic>())))
        );
    }

    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken)
    {
        if (!_documentManagerService.HasDocument(uri))
            return ArraySegment<Diagnostic>.Empty;

        return _diagnostics.TryGetValue(uri, out var diagnostics) ? diagnostics : ArraySegment<Diagnostic>.Empty;
    }
}
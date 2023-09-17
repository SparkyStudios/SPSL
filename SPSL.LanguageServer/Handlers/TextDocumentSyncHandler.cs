using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class TextDocumentSyncHandler : ITextDocumentSyncHandler
{
    private readonly ILanguageServerFacade _router;
    private readonly DocumentManagerService _documentManagerService;
    private readonly ConfigurationService _configurationService;
    private readonly SyntaxAnalyzerService _syntaxAnalyzerService;
    private readonly StaticAnalyzerService _staticAnalyzerService;
    private readonly DocumentDiagnosticService _documentDiagnosticService;

    private readonly DocumentSelector _documentSelector;

    public TextDocumentSyncHandler
    (
        ILanguageServerFacade router,
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        SyntaxAnalyzerService syntaxAnalyzerService,
        StaticAnalyzerService staticAnalyzerService,
        DocumentDiagnosticService documentDiagnosticService,
        DocumentSelector documentSelector
    )
    {
        _router = router;
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _syntaxAnalyzerService = syntaxAnalyzerService;
        _staticAnalyzerService = staticAnalyzerService;
        _documentDiagnosticService = documentDiagnosticService;
        _documentSelector = documentSelector;

        _documentManagerService.DataUpdated += DocumentManagerServiceOnDocumentContentChanged;

        _syntaxAnalyzerService.DataUpdated += SyntaxAnalyzerServiceOnDataUpdated;
        _staticAnalyzerService.DataUpdated += StaticAnalyzerServiceOnDataUpdated;
        
        _documentDiagnosticService.DataUpdated += DocumentDiagnosticServiceOnDataUpdated;
    }

    private void DocumentDiagnosticServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<Container<Diagnostic>> e)
    {
        if (!_documentManagerService.HasDocument(e.Uri))
            return;

        Document document = _documentManagerService.GetData(e.Uri);

        _router.TextDocument.PublishDiagnostics
        (
            new()
            {
                Uri = document.Uri,
                Version = document.Version,
                Diagnostics = e.Data
            }
        );
    }

    private void StaticAnalyzerServiceOnDataUpdated
    (
        object? sender,
        ProviderDataUpdatedEventArgs<Container<Diagnostic>> e
    )
    {
        _documentDiagnosticService.SetDiagnostics(e.Uri, nameof(StaticAnalyzerService), e.Data);
    }

    private void SyntaxAnalyzerServiceOnDataUpdated
    (
        object? sender,
        ProviderDataUpdatedEventArgs<Container<Diagnostic>> e
    )
    {
        _documentDiagnosticService.SetDiagnostics(e.Uri, nameof(SyntaxAnalyzerService), e.Data);
    }

    private void DocumentManagerServiceOnDocumentContentChanged(object? sender, ProviderDataUpdatedEventArgs<Document> e)
    {
        _router.Window.LogInfo($"Document content changed: {e.Uri}");
    }

    TextDocumentOpenRegistrationOptions
        IRegistration<TextDocumentOpenRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability capability,
            ClientCapabilities clientCapabilities
        )
    {
        return new()
        {
            DocumentSelector = _documentSelector,
        };
    }

    TextDocumentChangeRegistrationOptions
        IRegistration<TextDocumentChangeRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability capability,
            ClientCapabilities clientCapabilities
        )
    {
        return new()
        {
            DocumentSelector = _documentSelector,
            SyncKind = TextDocumentSyncKind.Incremental,
        };
    }

    TextDocumentSaveRegistrationOptions
        IRegistration<TextDocumentSaveRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability capability,
            ClientCapabilities clientCapabilities
        )
    {
        return new()
        {
            DocumentSelector = _documentSelector,
            IncludeText = true
        };
    }

    TextDocumentCloseRegistrationOptions
        IRegistration<TextDocumentCloseRegistrationOptions, SynchronizationCapability>.GetRegistrationOptions
        (
            SynchronizationCapability capability,
            ClientCapabilities clientCapabilities
        )
    {
        return new()
        {
            DocumentSelector = _documentSelector
        };
    }

    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);
        document.Version = request.TextDocument.Version;
        document.Update(request.ContentChanges, request.TextDocument.Version);

        _documentManagerService.SetData(request.TextDocument.Uri, document);

        return Unit.Task;
    }

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);
        document.Version = request.TextDocument.Version;
        document.SetText(request.TextDocument.Text);

        _documentManagerService.SetData(request.TextDocument.Uri, document);

        return Unit.Task;
    }


    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documentManagerService.RemoveDocument(request.TextDocument.Uri);

        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);
        document.SetText(request.Text ?? "");

        _documentManagerService.SetData(request.TextDocument.Uri, document);

        return Unit.Task;
    }

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new(uri, "file", "spsl");
    }
}
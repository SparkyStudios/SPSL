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
    private readonly SyntaxDiagnosticService _syntaxDiagnosticService;

    private readonly DocumentSelector _documentSelector = new
    (
        new DocumentFilter
        {
            Pattern = "**/*.spsl*",
            Scheme = "file",
            Language = "spsl"
        }
    );

    public TextDocumentSyncHandler
    (
        ILanguageServerFacade router,
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        SyntaxDiagnosticService syntaxDiagnosticService
    )
    {
        _router = router;
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _syntaxDiagnosticService = syntaxDiagnosticService;

        _documentManagerService.DocumentContentChanged += DocumentManagerServiceOnDocumentContentChanged;

        _syntaxDiagnosticService.DataUpdated += SyntaxDiagnosticServiceOnDataUpdated;
    }

    private void SyntaxDiagnosticServiceOnDataUpdated
    (
        object? sender,
        ProviderDataUpdatedEventArgs<Container<Diagnostic>> e
    )
    {
        if (!_documentManagerService.HasDocument(e.Uri))
            return;

        Document document = _documentManagerService.GetDocument(e.Uri);

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

    private void DocumentManagerServiceOnDocumentContentChanged(object? sender, DocumentEventArgs e)
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
        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri);
        document.Version = request.TextDocument.Version;
        document.Update(request.ContentChanges, request.TextDocument.Version);

        _documentManagerService.UpdateDocument(request.TextDocument.Uri, document);

        return Unit.Task;
    }

    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri);
        document.Version = request.TextDocument.Version;
        document.SetText(request.TextDocument.Text);

        _documentManagerService.UpdateDocument(request.TextDocument.Uri, document);

        return Unit.Task;
    }


    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _documentManagerService.RemoveDocument(request.TextDocument.Uri);

        return Unit.Task;
    }

    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri);
        document.SetText(request.Text ?? "");

        _documentManagerService.UpdateDocument(request.TextDocument.Uri, document);

        return Unit.Task;
    }

    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new(uri, "file", "spsl");
    }
}
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Symbols;
using SPSL.Language.Symbols.Modifiers;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;
using SPSL.LanguageServer.Utils;

namespace SPSL.LanguageServer.Handlers;

public class CompletionHandler : ICompletionHandler, ICompletionResolveHandler
{
    public Guid Id => Guid.Parse("710c73d8-440c-4f6e-a93f-85b958c9b6df");

    private readonly SymbolProviderService _symbolProviderService;
    private readonly DocumentManagerService _documentManagerService;

    private readonly DocumentSelector _documentSelector = new
    (
        new DocumentFilter
        {
            Pattern = "**/*.spsl*",
            Scheme = "file",
            Language = "spsl"
        }
    );

    public CompletionHandler
    (
        SymbolProviderService symbolProviderService,
        DocumentManagerService documentManagerService
    )
    {
        _symbolProviderService = symbolProviderService;
        _documentManagerService = documentManagerService;
    }

    public CompletionRegistrationOptions GetRegistrationOptions
    (
        CompletionCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new()
        {
            DocumentSelector = _documentSelector,
        };
    }

    public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri);
        SymbolTable? symbolTable = _symbolProviderService.GetData(request.TextDocument.Uri);

        if (symbolTable == null)
            return Task.FromResult(new CompletionList());

        SymbolTable scope = SymbolTableUtils.GetParentSymbolTableAtOffset
        (
            request.TextDocument.Uri,
            symbolTable,
            document.OffsetAt(request.Position)
        );

        var parentItem = scope.Parent?.Symbols.Select(symbol => new CompletionItem
        {
            Kind = symbol.Type == SymbolType.Parameter ? CompletionItemKind.Variable : CompletionItemKind.Keyword,
            Label = symbol.Name,
            Detail = symbol.Type == SymbolType.Parameter ? (symbol.Modifiers.First(modifier =>
                modifier is SymbolTypeModifier) as SymbolTypeModifier)!.Name : symbol.Type.ToString(),
        });

        
        var items = scope.Symbols.Select(symbol => new CompletionItem
        {
            Kind = symbol.Type == SymbolType.Parameter ? CompletionItemKind.Variable : CompletionItemKind.Keyword,
            Label = symbol.Name,
            InsertTextFormat = InsertTextFormat.PlainText,
            Detail = symbol.Type == SymbolType.Parameter ? (symbol.Modifiers.First(modifier =>
                modifier is SymbolTypeModifier) as SymbolTypeModifier)!.Name : symbol.Type.ToString(),
        });
        
        items = items.Concat(parentItem?? Array.Empty<CompletionItem>()).ToList();

        return Task.FromResult
        (
            new CompletionList
            (
                items
            )
        );
    }

    public void SetCapability(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
    }
}
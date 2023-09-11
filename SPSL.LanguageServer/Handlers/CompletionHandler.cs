using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Analysis.Common;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Analysis.Symbols.Modifiers;
using SPSL.Language.Parsing.AST;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class CompletionHandler : ICompletionHandler, ICompletionResolveHandler
{
    public Guid Id => Guid.Parse("710c73d8-440c-4f6e-a93f-85b958c9b6df");

    private readonly SymbolProviderService _symbolProviderService;
    private readonly DocumentManagerService _documentManagerService;
    private readonly AstProviderService _astProviderService;

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
        DocumentManagerService documentManagerService,
        AstProviderService astProviderService)
    {
        _symbolProviderService = symbolProviderService;
        _documentManagerService = documentManagerService;
        _astProviderService = astProviderService;
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
        if (request.Data == null)
            return Task.FromResult(request);

        Ast? ast = _astProviderService.GetData(request.Data["document"].Value<string>());
        INode? node = ast?.ResolveNode(request.Data["document"].Value<string>(), request.Data["symbol"].Value<int>());

        if (node == null)
            return Task.FromResult(request);

        return Task.FromResult
        (
            new CompletionItem
            {
                Kind = request.Kind,
                Detail = request.Detail,
                FilterText = request.FilterText,
                Data = request.Data,
                Documentation = node is IDocumented documented ? documented.Documentation : string.Empty,
                Label = request.Label,
                SortText = request.SortText,
                TextEdit = request.TextEdit,
                InsertText = request.InsertText
            }
        );
    }

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);

        SymbolTable? root = _symbolProviderService.GetData(request.TextDocument.Uri);
        SymbolTable? scope = root?.FindEnclosingScope
        (
            request.TextDocument.Uri.ToString(),
            document.OffsetAt(request.Position)
        );

        if (scope == null)
            return Task.FromResult(new CompletionList());

        var items = new List<CompletionItem>();
        SymbolTable? current = scope;
        while (current != null)
        {
            items.AddRange(CollectCompletionItems(current, current == scope));
            current = current.Parent;
        }

        return Task.FromResult(new CompletionList(items));

        IEnumerable<CompletionItem> CollectCompletionItems(SymbolTable symbolTable, bool isEnclosingScope)
        {
            return symbolTable.Symbols
                .Where(symbol => symbol.Type is not SymbolType.Scope and not SymbolType.Identifier &&
                                 (!isEnclosingScope || symbol.End <= document.OffsetAt(request.Position))
                )
                .Select(symbol =>
                    new CompletionItem
                    {
                        Kind = symbol.Type switch
                        {
                            SymbolType.Parameter or SymbolType.LocalVariable => CompletionItemKind.Variable,
                            SymbolType.Function => CompletionItemKind.Function,
                            SymbolType.Buffer => CompletionItemKind.Class,
                            SymbolType.Constructor => CompletionItemKind.Constructor,
                            SymbolType.Constant => CompletionItemKind.Constant,
                            SymbolType.Enum => CompletionItemKind.Enum,
                            SymbolType.Fragment => CompletionItemKind.Class,
                            SymbolType.Interface => CompletionItemKind.Interface,
                            SymbolType.Material => CompletionItemKind.Class,
                            SymbolType.Namespace => CompletionItemKind.Module,
                            SymbolType.Permutation => CompletionItemKind.Variable,
                            SymbolType.Property => CompletionItemKind.Property,
                            SymbolType.Shader => CompletionItemKind.Class,
                            SymbolType.Struct => CompletionItemKind.Class,
                            _ => CompletionItemKind.Text
                        },
                        Label = symbol.Name,
                        InsertTextFormat = InsertTextFormat.PlainText,
                        SortText = $"{document.Length - symbol.Start}_{symbol.Name}_{symbol.Type}",
                        Data = new JObject
                        {
                            ["document"] = request.TextDocument.Uri.ToString(),
                            ["symbol"] = symbol.Start
                        },
                        Detail = symbol.Type switch
                        {
                            SymbolType.Parameter or SymbolType.LocalVariable => (symbol.Modifiers.First(modifier =>
                                modifier is SymbolTypeModifier) as SymbolTypeModifier)!.Name,
                            _ => symbol.Type.ToString()
                        },
                    }
                );
        }
    }

    public void SetCapability(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        capability.CompletionItem = new()
        {
            DeprecatedSupport = true,
            DocumentationFormat = new(MarkupKind.Markdown),
            ResolveSupport = new()
            {
                Properties = new("documentation")
            },
            SnippetSupport = true,
        };
    }
}
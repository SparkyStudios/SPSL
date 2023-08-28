using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Symbols;
using SPSL.Language.Utils;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using Type = SPSL.Language.AST.Type;

namespace SPSL.LanguageServer.Handlers;

public class HoverHandler : IHoverHandler
{
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

    public HoverHandler
    (
        SymbolProviderService symbolProviderService,
        DocumentManagerService documentManagerService,
        AstProviderService astProviderService
    )
    {
        _symbolProviderService = symbolProviderService;
        _documentManagerService = documentManagerService;
        _astProviderService = astProviderService;
    }

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        Ast? ast = _astProviderService.GetData(request.TextDocument.Uri);

        if (ast == null)
            return Task.FromResult<Hover?>(null);

        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri.ToString());
        INode? node = ast.ResolveNode(request.TextDocument.Uri.ToString(), document.OffsetAt(request.Position));

        if (node == null)
            return Task.FromResult<Hover?>(null);

        return Task.FromResult(CreateHover(document, node));
    }

    private Hover? CreateHover(Document document, INode node)
    {
        return node switch
        {
            INamespaceChild namespaceChild => CreateHover(document, namespaceChild),
            TypeProperty property => CreateHover(document, property),
            TypeFunction function => CreateHover(document, function),
            FunctionArgument argument => CreateHover(document, argument),
            ILiteral literal => CreateHover(document, literal),
            Identifier identifier => CreateHover(document, identifier),
            _ => new()
            {
                Contents = new(new MarkedString($"Not yet implemented ({node})")),
                Range = new()
                {
                    Start = document.PositionAt(node.Start),
                    End = document.PositionAt(node.End + 1)
                }
            }
        };
    }

    public HoverRegistrationOptions GetRegistrationOptions
    (
        HoverCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        capability.ContentFormat ??= new(MarkupKind.Markdown);

        return new()
        {
            DocumentSelector = _documentSelector,
        };
    }

    private static Hover CreateHover(Document document, INamespaceChild symbol)
    {
        return new()
        {
            Contents = new
            (
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"""
                             ```spsl
                             {DeclarationString.From(symbol)}
                             ```
                             ---
                             {symbol.Documentation}
                             """,
                }
            ),
            Range = new()
            {
                Start = document.PositionAt(symbol.Name.Start),
                End = document.PositionAt(symbol.Name.End + 1)
            }
        };
    }

    private static Hover CreateHover(Document document, TypeProperty symbol)
    {
        return new()
        {
            Contents = new
            (
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"""
                             ```spsl
                             {DeclarationString.From(symbol)}
                             ```
                             ---
                             {symbol.Documentation}
                             """,
                }
            ),
            Range = new()
            {
                Start = document.PositionAt(symbol.Name.Start),
                End = document.PositionAt(symbol.Name.End + 1)
            }
        };
    }

    private static Hover CreateHover(Document document, TypeFunction symbol)
    {
        return new()
        {
            Contents = new
            (
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"""
                             ```spsl
                             {DeclarationString.From(symbol)}
                             ```
                             ---
                             {symbol.Documentation}
                             """,
                }
            ),
            Range = new()
            {
                Start = document.PositionAt(symbol.Name.Start),
                End = document.PositionAt(symbol.Name.End + 1)
            }
        };
    }

    private static Hover CreateHover(Document document, FunctionArgument symbol)
    {
        return new()
        {
            Contents = new
            (
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"""
                             ```spsl
                             {DeclarationString.From(symbol)}
                             ```
                             ---
                             {symbol.Documentation}
                             """,
                }
            ),
            Range = new()
            {
                Start = document.PositionAt(symbol.Name.Start),
                End = document.PositionAt(symbol.Name.End + 1)
            }
        };
    }

    private static Hover CreateHover(Document document, ILiteral literal)
    {
        return new()
        {
            Contents = new
            (
                new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = $"""
                             ```spsl
                             {literal}
                             ```
                             ---
                             Constant value
                             """,
                }
            ),
            Range = new()
            {
                Start = document.PositionAt(literal.Start),
                End = document.PositionAt(literal.End + 1)
            }
        };
    }

    private Hover? CreateHover(Document document, Identifier identifier)
    {
        switch (identifier.Parent)
        {
            case null:
                return null;
            case BasicExpression:
            {
                SymbolTable? table = _symbolProviderService.GetData(document.Uri);
                Symbol? symbol = table?.Resolve(document.Uri.ToString(), identifier.Value, identifier.Start);

                if (symbol == null)
                    return null;
                
                Ast? ast = _astProviderService.GetData(document.Uri);
                INode? node = ast?.ResolveNode(document.Uri.ToString(), symbol.Start);
                
                return node == null ? null : CreateHover(document, node);
            }
            default:
                return CreateHover(document, identifier.Parent);
        }
    }
}
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

    private static Hover? CreateHover(Document document, INode node)
    {
        return node switch
        {
            PermutationVariable variable => CreateHover(document, variable),
            Type type => CreateHover(document, type),
            TypeProperty property => CreateHover(document, property),
            ILiteral literal => CreateHover(document, literal),
            Identifier identifier => CreateHover(document, identifier),
            _ => new()
            {
                Contents = new(new MarkedString("Not yet implemented")),
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

    private static Hover CreateHover(Document document, PermutationVariable symbol)
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

    private static Hover CreateHover(Document document, Type symbol)
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

    private static Hover? CreateHover(Document document, Identifier identifier)
    {
        return identifier.Parent == null ? null : CreateHover(document, identifier.Parent);
    }
}
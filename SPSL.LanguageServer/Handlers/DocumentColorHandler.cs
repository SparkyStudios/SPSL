using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Parsing.Visitors;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class DocumentColorHandler : IDocumentColorHandler
{
    private class ColorInstanceListener : SPSLBaseListener
    {
        private readonly string _documentUri;

        public ColorInstanceListener(string documentUri)
        {
            _documentUri = documentUri;
        }

        public List<NewInstanceExpression> Instances { get; } = new();

        public override void EnterNewInstanceExpression(SPSLParser.NewInstanceExpressionContext context)
        {
            var instance = (NewInstanceExpression)context.Accept(new ExpressionVisitor(_documentUri))!;

            if (instance.Type is BuiltInDataType { Type: BuiltInDataTypeKind.Color3 or BuiltInDataTypeKind.Color4 } &&
                instance.Parameters.All(p => p.Expression is IPrimitiveExpression))
                Instances.Add(instance);
        }
    }

    private readonly TokenProviderService _tokenProviderService;
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

    public DocumentColorHandler
    (
        TokenProviderService tokenProviderService,
        DocumentManagerService documentManagerService
    )
    {
        _tokenProviderService = tokenProviderService;
        _documentManagerService = documentManagerService;
    }

    public Task<Container<ColorInformation>> Handle
    (
        DocumentColorParams request,
        CancellationToken cancellationToken
    )
    {
        ParserRuleContext? context = _tokenProviderService.GetData(request.TextDocument.Uri);
        if (context == null)
            return Task.FromResult<Container<ColorInformation>>(new());

        Document document = _documentManagerService.GetDocument(request.TextDocument.Uri);

        var listener = new ColorInstanceListener(request.TextDocument.Uri.ToString());
        ParseTreeWalker.Default.Walk(listener, context);

        var result = listener.Instances.Select(instance => new ColorInformation
            {
                Range = new()
                {
                    Start = document.PositionAt(instance.Start),
                    End = document.PositionAt(instance.End + 1)
                },
                Color = new()
                {
                    Red = Convert.ToDouble((instance.Parameters[0].Expression as ILiteral)?.Value),
                    Green = Convert.ToDouble((instance.Parameters[1].Expression as ILiteral)?.Value),
                    Blue = Convert.ToDouble((instance.Parameters[2].Expression as ILiteral)?.Value),
                    Alpha = instance.Type is BuiltInDataType { Type: BuiltInDataTypeKind.Color4 }
                        ? Convert.ToDouble((instance.Parameters[3].Expression as ILiteral)?.Value)
                        : 1.0
                }
            })
            .ToList();

        return Task.FromResult<Container<ColorInformation>>(result);
    }

    public DocumentColorRegistrationOptions GetRegistrationOptions
    (
        ColorProviderCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new()
        {
            Id = "87ef4408-5f63-4501-b4a6-307e1c4cee5d",
            DocumentSelector = _documentSelector,
        };
    }
}
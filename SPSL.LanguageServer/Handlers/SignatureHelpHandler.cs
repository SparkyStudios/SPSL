using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Parsing.AST;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class SignatureHelpHandler : ISignatureHelpHandler
{
    private readonly DocumentManagerService _documentManagerService;
    private readonly AstProviderService _astProviderService;
    private readonly SymbolProviderService _symbolProviderService;

    private readonly DocumentSelector _documentSelector = new
    (
        new DocumentFilter
        {
            Pattern = "**/*.spsl*",
            Scheme = "file",
            Language = "spsl"
        }
    );

    public SignatureHelpHandler
    (
        DocumentManagerService documentManagerService,
        AstProviderService astProviderService,
        SymbolProviderService symbolProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _astProviderService = astProviderService;
        _symbolProviderService = symbolProviderService;
    }

    public Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);
        Ast? ast = _astProviderService.GetData(document.Uri);
        INode? iNode = ast?.ResolveNode(document.Uri.ToString(), document.OffsetAt(request.Position));

        if (iNode is not InvocationExpression invocation)
            return Task.FromResult<SignatureHelp?>(null);

        SymbolTable? rootTable = _symbolProviderService.GetData(document.Uri);

        if (rootTable == null)
            return Task.FromResult<SignatureHelp?>(null);

        SymbolTable? scope = rootTable.FindEnclosingScope(document.Uri.ToString(), document.OffsetAt(request.Position));
        Symbol? symbol = scope?.Resolve(invocation.Name.Name); // TODO: handle symbol from another namespace

        if (symbol == null)
            return Task.FromResult<SignatureHelp?>(null);

        INode? fNode = ast?.ResolveNode(document.Uri.ToString(), symbol.Start);

        if (fNode == null)
            return Task.FromResult<SignatureHelp?>(null);

        SignatureHelp? helper = (fNode is Identifier fNodeName ? fNodeName.Parent : fNode) switch
        {
            ShaderFunction sFunction => new()
            {
                ActiveParameter = 0,
                ActiveSignature = 0,
                Signatures = new(new SignatureInformation
                {
                    ActiveParameter = 0,
                    Label = sFunction.Name.Value,
                    Documentation = sFunction.Documentation,
                    Parameters = sFunction.Function.Head.Signature.Arguments.Select(p =>
                            new ParameterInformation { Label = p.Name.Value, Documentation = p.Documentation })
                        .ToList()
                })
            },
            TypeFunction tFunction => new()
            {
                ActiveParameter = 0,
                ActiveSignature = 0,
                Signatures = new(new SignatureInformation
                {
                    ActiveParameter = 0,
                    Label = tFunction.Name.Value,
                    Documentation = tFunction.Documentation,
                    Parameters = tFunction.Function.Head.Signature.Arguments.Select(p =>
                            new ParameterInformation { Label = p.Name.Value, Documentation = p.Documentation })
                        .ToList()
                })
            },
            Function function => new()
            {
                ActiveParameter = 0,
                ActiveSignature = 0,
                Signatures = new(new SignatureInformation
                {
                    ActiveParameter = 0,
                    Label = function.Name.Value,
                    Parameters = function.Head.Signature.Arguments.Select(p =>
                            new ParameterInformation { Label = p.Name.Value, Documentation = p.Documentation })
                        .ToList()
                })
            },
            FunctionHead functionHead => new()
            {
                ActiveParameter = 1,
                ActiveSignature = 1,
                Signatures = new(new SignatureInformation
                {
                    ActiveParameter = 1,
                    Label = functionHead.Name.Value,
                    Parameters = functionHead.Signature.Arguments.Select(p =>
                            new ParameterInformation { Label = p.Name.Value, Documentation = p.Documentation })
                        .ToList()
                })
            },
            _ => null
        };

        return Task.FromResult(helper);
    }

    public SignatureHelpRegistrationOptions GetRegistrationOptions
    (
        SignatureHelpCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        capability.ContextSupport = true;
        capability.SignatureInformation = new()
        {
            DocumentationFormat = new(MarkupKind.Markdown),
            ParameterInformation = new()
            {
                LabelOffsetSupport = true,
            },
            ActiveParameterSupport = true
        };

        return new()
        {
            DocumentSelector = _documentSelector,
            TriggerCharacters = new("("),
            RetriggerCharacters = new(",")
        };
    }
}
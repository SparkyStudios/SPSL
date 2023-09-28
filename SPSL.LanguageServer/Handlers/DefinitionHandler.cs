using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Parsing.AST;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class DefinitionHandler : IDefinitionHandler
{
    private readonly WorkspaceService _workspaceService;
    private readonly DocumentManagerService _documentManagerService;

    private readonly DocumentSelector _documentSelector;

    public DefinitionHandler
    (
        DocumentSelector documentSelector,
        WorkspaceService workspaceService,
        DocumentManagerService documentManagerService
    )
    {
        _documentSelector = documentSelector;
        _workspaceService = workspaceService;
        _documentManagerService = documentManagerService;
    }

    public Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
    {
        Document document = _documentManagerService.GetData(request.TextDocument.Uri);
        int offset = document.OffsetAt(request.Position);

        INode? node = _workspaceService.WorkspaceAst.ResolveNode
        (
            request.TextDocument.Uri.ToString(),
            offset
        );

        if (node == null)
            return Task.FromResult<LocationOrLocationLinks>(new());

        switch (node)
        {
            case Identifier identifier:
            {
                Symbol? symbol = _workspaceService.WorkspaceSymbolTable.LookupInCurrentAndChildTables(identifier.Value);
                if (symbol == null)
                    return Task.FromResult<LocationOrLocationLinks>(new());

                // TODO: According to the parent of the identifier, we should find the definition of the symbol.
                // For now, we just return the symbol itself.

                Document symbolDocument =
                    _documentManagerService.GetData(DocumentUri.From(symbol.Source));

                return Task.FromResult<LocationOrLocationLinks>
                (
                    new
                    (
                        new LocationOrLocationLink
                        (
                            new Location
                            {
                                Range = new()
                                {
                                    Start = symbolDocument.PositionAt(symbol.Start),
                                    End = symbolDocument.PositionAt(symbol.End + 1)
                                },
                                Uri = symbolDocument.Uri
                            }
                        )
                    )
                );
            }
        }

        return Task.FromResult<LocationOrLocationLinks>(new());
    }

    public DefinitionRegistrationOptions GetRegistrationOptions
    (
        DefinitionCapability capability,
        ClientCapabilities clientCapabilities
    )
    {
        return new()
        {
            DocumentSelector = _documentSelector,
        };
    }
}
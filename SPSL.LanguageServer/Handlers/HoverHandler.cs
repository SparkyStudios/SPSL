using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Handlers;

public class HoverHandler : IHoverHandler
{
    private readonly DocumentSelector _documentSelector = new
    (
        new DocumentFilter
        {
            Pattern = "**/*.spsl*",
            Scheme = "file",
            Language = "spsl"
        }
    );

    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult
        (
            new Hover
            {
                Contents = new
                (
                    new MarkupContent
                    {
                        Kind = MarkupKind.Markdown,
                        Value = """
                                # Test

                                This is **test**. With __MD__
                                """,
                    }
                ),
                Range = new Range
                {
                    Start = request.Position,
                    End = request.Position,
                }
            }
        )!;
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
}
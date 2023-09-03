using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace SPSL.LanguageServer.Handlers;

public class ColorPresentationHandler : IColorPresentationHandler
{
    public Task<Container<ColorPresentation>> Handle
    (
        ColorPresentationParams request,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult
        (
            new Container<ColorPresentation>
            (
                new ColorPresentation
                {
                    Label =
                        $"color4({request.Color.Red:F2}, {request.Color.Green:F2}, {request.Color.Blue:F2}, {request.Color.Alpha:F2})",
                    TextEdit = new()
                    {
                        NewText =
                            $"color4({request.Color.Red:F2}, {request.Color.Green:F2}, {request.Color.Blue:F2}, {request.Color.Alpha:F2})",
                        Range = request.Range
                    }
                },
                new ColorPresentation
                {
                    Label = $"color3({request.Color.Red:F2}, {request.Color.Green:F2}, {request.Color.Blue:F2})",
                    TextEdit = new()
                    {
                        NewText = $"color3({request.Color.Red:F2}, {request.Color.Green:F2}, {request.Color.Blue:F2})",
                        Range = request.Range
                    }
                }
            )
        );
    }

    public void SetCapability(ColorProviderCapability capability, ClientCapabilities clientCapabilities)
    {
        capability.DynamicRegistration = true;
    }
}

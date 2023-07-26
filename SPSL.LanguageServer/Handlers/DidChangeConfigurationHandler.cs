using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class DidChangeConfigurationHandler : IDidChangeConfigurationHandler
{
    private readonly ConfigurationService _configurationService;

    public DidChangeConfigurationHandler(ConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    public Task<Unit> Handle(DidChangeConfigurationParams request, CancellationToken cancellationToken)
    {
        if (_configurationService.HasConfigurationCapability)
        {
            _configurationService.DocumentSettings.Clear();
        }
        else
        {
            _configurationService.GlobalSettings = request.Settings?["spsl"] ?? JObject.Parse("{}");
        }
        
        return Unit.Task;
    }

    public void SetCapability(DidChangeConfigurationCapability capability, ClientCapabilities clientCapabilities)
    {
        capability.DynamicRegistration = true;
    }
}
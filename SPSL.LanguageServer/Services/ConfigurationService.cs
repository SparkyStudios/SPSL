using System.Collections.Concurrent;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace SPSL.LanguageServer.Services;

public class ConfigurationService
{
    private readonly ILanguageServerFacade _languageServer;

    public ConfigurationService(ILanguageServerFacade languageServer)
    {
        _languageServer = languageServer;
    }

    public ConcurrentDictionary<DocumentUri, JToken> DocumentSettings { get; } = new();

    public JToken GlobalSettings { get; set; } = JObject.Parse("{}");

    /// <summary>
    /// Checks if the client has configuration capabilities.
    /// </summary>
    public bool HasConfigurationCapability { get; set; } = false;

    /// <summary>
    /// Checks if the client supports workspace folders.
    /// </summary>
    public bool HasWorkspaceFolderCapability { get; set; } = false;

    /// <summary>
    /// Checks if the client supports diagnostic related information.
    /// </summary>
    public bool HasDiagnosticRelatedInformationCapability { get; set; } = false;

    public async Task<JToken> GetDocumentSettingsAsync(DocumentUri uri)
    {
        var result = await _languageServer.Workspace.RequestConfiguration
        (
            new()
            {
                Items = new
                (
                    new ConfigurationItem()
                    {
                        ScopeUri = uri,
                        Section = "spsl"
                    }
                )
            }
        );

        return result.ElementAt(0);
    }
}
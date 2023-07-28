using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Server;
using SPSL.LanguageServer.Core;
using SPSL.LanguageServer.Handlers;
using SPSL.LanguageServer.Services;

static void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<DocumentManagerService>();
    services.AddSingleton<ConfigurationService>();
    services.AddSingleton<TokenProviderService>();
    services.AddSingleton<AstProviderService>();
    services.AddSingleton<SyntaxDiagnosticService>();
    services.AddSingleton<SymbolProviderService>();
}

static Task<InitializeResult> OnInitialize
(
    ILanguageServer languageServer,
    InitializeParams request,
    CancellationToken cancellationToken
)
{
    ClientCapabilities? capabilities = request.Capabilities;

    var configService = languageServer.Services.GetService<ConfigurationService>()!;

    configService.HasConfigurationCapability = capabilities?.Workspace is { Configuration.Value: true };
    configService.HasWorkspaceFolderCapability = capabilities?.Workspace is { WorkspaceFolders.Value: true };
    configService.HasDiagnosticRelatedInformationCapability = capabilities?.TextDocument?.PublishDiagnostics.Value is
    {
        RelatedInformation: true
    };

    return Task.FromResult
    (
        new InitializeResult
        {
            Capabilities = new()
            {
                TextDocumentSync = new
                (
                    new TextDocumentSyncOptions
                    {
                        Change = TextDocumentSyncKind.Incremental,
                        OpenClose = true,
                        Save = true
                    }
                ),
                CompletionProvider = new()
                {
                    ResolveProvider = true,
                    TriggerCharacters = new(".")
                },
                HoverProvider = new(true),
                Workspace = new()
                {
                    WorkspaceFolders = new()
                    {
                        Supported = configService.HasWorkspaceFolderCapability
                    }
                }
            },
            ServerInfo = new()
            {
                Name = "SPSL Language Server",
                Version = "0.0.1"
            }
        }
    );
}

Debugger.Launch();

LanguageServer? server = null;

try
{
    server = await LanguageServer.From(options =>
        options
            .WithInput(Console.OpenStandardInput())
            .WithOutput(Console.OpenStandardOutput())
            .ConfigureLogging(
                x => x
                    // .AddSerilog(Log.Logger)
                    .AddLanguageProtocolLogging()
                    .SetMinimumLevel(LogLevel.Debug)
            )
            .WithServices(ConfigureServices)
            .WithHandler<TextDocumentSyncHandler>()
            .WithHandler<HoverHandler>()
            .WithHandler<CompletionHandler>()
            .OnInitialize(OnInitialize)
    );
}
catch (Exception ex)
{
    server?.Dispose();
    await Console.Error.WriteLineAsync(ex.ToString());
    return;
}

try
{
    await server.WaitForExit;
}
catch (Exception ex)
{
    await Console.Error.WriteLineAsync(ex.ToString());
}
finally
{
    server.Dispose();
}
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using SPSL.Language.Parsing.AST;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// Manages the entire workspace.
/// </summary>
public class WorkspaceService : IOnLanguageServerInitialized
{
    private readonly DocumentManagerService _documentManagerService;
    private readonly AstProviderService _astProviderService;
    private readonly TokenProviderService _tokenProviderService;

    private readonly Ast _ast = new();

    public WorkspaceService
    (
        DocumentManagerService documentManagerService,
        AstProviderService astProviderService,
        TokenProviderService tokenProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _astProviderService = astProviderService;
        _tokenProviderService = tokenProviderService;

        _astProviderService.DataUpdated += AstProviderServiceOnDataUpdated;
    }

    private void AstProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<Ast> e)
    {
        // Merge the generated AST with the existing one.
        MergeAst(e.Data);
    }

    public void MergeAst(Ast ast)
    {
        _ast.Merge(ast);
    }

    public Ast GetAst()
    {
        return _ast;
    }

    #region IOnLanguageServerInitialized Implementation

    public Task OnInitialized
    (
        ILanguageServer server,
        InitializeParams request,
        InitializeResult result,
        CancellationToken cancellationToken
    )
    {
        server.LogMessage(new()
        {
            Message = "SPSL Language Server Initialized",
            Type = MessageType.Info
        });

        ThreadPool.QueueUserWorkItem
        (
            ((Action<ILanguageServer>)ScanWorkspace).Debounce(TimeSpan.FromMilliseconds(500)),
            server,
            true
        );

        return Task.CompletedTask;

        async void ScanWorkspace(ILanguageServer languageServer)
        {
            CancellationToken workCancellationToken = new();

            using IWorkDoneObserver work = await languageServer.WorkDoneManager.Create
            (
                new() { Title = "Parsing Workspace", Message = "Initializing...", Cancellable = false, Percentage = 0 },
                _ =>
                {
                    languageServer.Window.LogMessage(new() { Message = "An error occurred", Type = MessageType.Info });

                    return new() { Message = "An error occured" };
                }, () =>
                {
                    languageServer.Window.LogMessage(new() { Message = "Work complete", Type = MessageType.Info });

                    return new() { Message = "Work completed" };
                },
                workCancellationToken
            );

            var processed = 0;

            if (languageServer.Workspace.ClientSettings.WorkspaceFolders is null) return;

            foreach (WorkspaceFolder folder in languageServer.Workspace.ClientSettings.WorkspaceFolders)
            {
                if (workCancellationToken.IsCancellationRequested) return;

                string[] files = Directory.GetFiles(folder.Uri.Path, "*.spsl*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    if (workCancellationToken.IsCancellationRequested) return;

                    try
                    {
                        languageServer.Window.LogMessage(new()
                            { Message = Path.GetFileName(file), Type = MessageType.Info });

                        work.OnNext(new WorkDoneProgressReport
                            { Message = Path.GetFileName(file), Percentage = processed * 100 / files.Length });

                        {
                            using var spsl = new StreamReader(file);

                            Document document = _documentManagerService.GetData(DocumentUri.FromFileSystemPath(file));

                            document.SetText(await spsl.ReadToEndAsync(workCancellationToken));
                            _documentManagerService.SetData(document.Uri, document);
                        }

                        processed++;
                    }
                    catch (Exception e)
                    {
                        work.OnError(e);
                        throw;
                    }
                }
            }
        }
    }

    #endregion
}
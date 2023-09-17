using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.WorkDone;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;
using SPSL.Language.Analysis.Common;
using SPSL.Language.Analysis.Symbols;
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
    private readonly SymbolProviderService _symbolProviderService;

    /// <summary>
    /// Gets the <see cref="Ast"/> instance storing data for the
    /// entire workspace.
    /// </summary>
    public Ast WorkspaceAst { get; } = new();

    /// <summary>
    /// Gets the <see cref="SymbolTable"/> instance storing data for the
    /// entire workspace.
    /// </summary>
    public SymbolTable WorkspaceSymbolTable { get; } = new()
    {
        Name = "__SPSL_GLOBAL_SYMBOL_TABLE__",
        Type = SymbolType.Scope
    };


    /// <summary>
    /// Whether the <see cref="WorkspaceService"/> has been initialized.
    ///
    /// The <see cref="WorkspaceService"/> is initialized when he has successfully parsed
    /// all the files of the opened workspace.
    /// </summary>
    public bool IsInitialized { get; private set; }

    public WorkspaceService
    (
        DocumentManagerService documentManagerService,
        AstProviderService astProviderService,
        SymbolProviderService symbolProviderService
    )
    {
        _documentManagerService = documentManagerService;
        _astProviderService = astProviderService;
        _symbolProviderService = symbolProviderService;

        _astProviderService.DataUpdated += AstProviderServiceOnDataUpdated;
        _symbolProviderService.DataUpdated += SymbolProviderServiceOnDataUpdated;
    }

    private void SymbolProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<SymbolTable> e)
    {
        // Merge the generated symbol table into the workspace symbol table.
        Merge(e.Data);
    }

    private void AstProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<Ast> e)
    {
        // Merge the generated AST with the existing one.
        Merge(e.Data);
    }

    public void Merge(Ast ast)
    {
        WorkspaceAst.Merge(ast);
    }

    public void Merge(SymbolTable symbolTable)
    {
        WorkspaceSymbolTable.Merge(symbolTable);
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
                new() { Title = "Loading Workspace", Message = "Initializing...", Cancellable = false, Percentage = 0 },
                e => new()
                {
                    Message = e.Message
                }, () =>
                {
                    IsInitialized = true;
                    return new() { Message = "Workspace successfully loaded" };
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
                        work.OnNext
                        (
                            new WorkDoneProgressReport
                            {
                                Message = Path.GetFileName(file),
                                Percentage = processed * 100 / files.Length
                            }
                        );

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
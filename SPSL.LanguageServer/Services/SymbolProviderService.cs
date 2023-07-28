using System.Collections.Concurrent;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.Listeners;
using SPSL.Language.Symbols;

namespace SPSL.LanguageServer.Services;

public class SymbolProviderService : IProviderService<SymbolTable>
{
    private readonly ConcurrentDictionary<DocumentUri, SymbolTable> _cache = new();

    private readonly TokenProviderService _tokenProviderService;

    public SymbolProviderService(TokenProviderService tokenProviderService)
    {
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        SymbolListener listener = new();
        ParseTreeWalker.Default.Walk(listener, e.Data);

        _cache.AddOrUpdate(e.Uri, listener.GlobalSymbolTable, (_, _) => listener.GlobalSymbolTable);
        DataUpdated?.Invoke(this, new(e.Uri, listener.GlobalSymbolTable));
    }

    #region IProviderService<SymbolTable> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<SymbolTable>>? DataUpdated;

    public SymbolTable? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out SymbolTable? symbolTable) ? symbolTable : null;
    }

    public void SetData(DocumentUri uri, SymbolTable data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data, (_, _) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
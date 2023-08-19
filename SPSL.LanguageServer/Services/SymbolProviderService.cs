using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.Symbols;
using SPSL.Language.Visitors;

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
        SymbolVisitor visitor = new(e.Uri.ToString());
        SymbolTable globalSymbolTable = (visitor.Visit(e.Data) as SymbolTable)!;

        _cache.AddOrUpdate(e.Uri, globalSymbolTable, (_, _) => globalSymbolTable);
        DataUpdated?.Invoke(this, new(e.Uri, globalSymbolTable));
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
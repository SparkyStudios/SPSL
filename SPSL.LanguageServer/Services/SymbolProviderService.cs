using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Exceptions;
using SPSL.Language.Symbols;
using SPSL.Language.Visitors;

namespace SPSL.LanguageServer.Services;

public class SymbolProviderService : IProviderService<SymbolTable>
{
    private readonly ConcurrentDictionary<DocumentUri, SymbolTable> _symbolsCache = new();
    private readonly ConcurrentDictionary<DocumentUri, List<Diagnostic>> _diagnosticsCache = new();

    private readonly TokenProviderService _tokenProviderService;

    public event EventHandler<ProviderDataUpdatedEventArgs<SemanticException>>? OnSemanticException;

    public SymbolProviderService(TokenProviderService tokenProviderService)
    {
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        SymbolVisitor visitor = new(e.Uri.ToString());

        try
        {
            var globalSymbolTable = (visitor.Visit(e.Data) as SymbolTable)!;
            SetData(e.Uri, globalSymbolTable);
        }
        catch (SemanticException ex)
        {
            OnSemanticException?.Invoke(this, new(e.Uri, ex));
        }
    }

    #region IProviderService<SymbolTable> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<SymbolTable>>? DataUpdated;

    public SymbolTable? GetData(DocumentUri uri)
    {
        return _symbolsCache.TryGetValue(uri, out SymbolTable? symbolTable) ? symbolTable : null;
    }

    public void SetData(DocumentUri uri, SymbolTable data, bool notify = true)
    {
        _symbolsCache.AddOrUpdate(uri, data, (_, _) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
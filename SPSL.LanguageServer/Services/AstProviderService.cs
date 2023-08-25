using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.AST;
using SPSL.Language.Visitors;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// <see cref="IProviderService{AST}" /> implementation that provides <see cref="Ast" /> data.
/// </summary>
public class AstProviderService : IProviderService<Ast>
{
    private readonly ConcurrentDictionary<DocumentUri, Ast> _cache = new();

    private readonly TokenProviderService _tokenProviderService;

    public AstProviderService(TokenProviderService tokenProviderService)
    {
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        AstVisitor visitor = new(e.Uri.ToString());
        Ast ast = visitor.Visit(e.Data);

        _cache.AddOrUpdate(e.Uri, ast, (_, _) => ast);
        DataUpdated?.Invoke(this, new(e.Uri, ast));
    }

    #region IProviderService<AST> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<Ast>>? DataUpdated;

    public Ast? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out Ast? ast) ? ast : null;
    }

    public void SetData(DocumentUri uri, Ast data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data, (_, _) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
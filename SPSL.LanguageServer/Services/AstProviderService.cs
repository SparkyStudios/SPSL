using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.AST;
using SPSL.Language.Visitors;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// <see cref="IProviderService{AST}" /> implementation that provides <see cref="AST" /> data.
/// </summary>
public class AstProviderService : IProviderService<AST>
{
    private readonly ConcurrentDictionary<DocumentUri, AST> _cache = new();

    private readonly TokenProviderService _tokenProviderService;

    public AstProviderService(TokenProviderService tokenProviderService)
    {
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        ASTVisitor visitor = new(e.Uri.ToString());
        AST ast = visitor.Visit(e.Data);

        _cache.AddOrUpdate(e.Uri, ast, (_, _) => ast);
        DataUpdated?.Invoke(this, new(e.Uri, ast));
    }

    #region IProviderService<AST> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<AST>>? DataUpdated;

    public AST? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out AST? ast) ? ast : null;
    }

    public void SetData(DocumentUri uri, AST data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data, (_, _) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
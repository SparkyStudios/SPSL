using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Visitors;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// <see cref="IProviderService{AST}" /> implementation that provides <see cref="Ast" /> data.
/// </summary>
public class AstProviderService : IProviderService<Ast>
{
    private readonly ConcurrentDictionary<DocumentUri, Ast> _cache = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly TokenProviderService _tokenProviderService;

    public AstProviderService(DocumentManagerService documentManagerService, TokenProviderService tokenProviderService)
    {
        _documentManagerService = documentManagerService;
        _tokenProviderService = tokenProviderService;

        _tokenProviderService.DataUpdated += TokenProviderServiceOnDataUpdated;
    }

    private void TokenProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<ParserRuleContext> e)
    {
        Document document = _documentManagerService.GetData(e.Uri);
        Parse(document, e.Data);
    }

    public Ast Parse(DocumentUri uri, bool notify = true)
    {
        Document document = _documentManagerService.GetData(uri);
        return Parse(document);
    }

    public Ast Parse(Document document, bool notify = true)
    {
        ParserRuleContext? tree = _tokenProviderService.GetData(document.Uri);
        return tree == null ? new() : Parse(document, tree, notify);
    }

    public Ast Parse(Document document, ParserRuleContext tree, bool notify = true)
    {
        AstVisitor visitor = new(document.Uri.ToString());
        Ast ast = visitor.Visit(tree);

        SetData(document.Uri, ast, notify);

        return ast;
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
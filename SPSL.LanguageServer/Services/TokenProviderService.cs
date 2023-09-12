using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.Core;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// <see cref="IProviderService{ParserRuleContext}"/> implementation that provides <see cref="ParserRuleContext"/> data.
/// </summary>
public class TokenProviderService : IProviderService<ParserRuleContext>
{
    private readonly ConcurrentDictionary<DocumentUri, ParserRuleContext> _cache = new();

    private readonly DocumentManagerService _documentManagerService;

    public event EventHandler<CollectParserErrorListenersEventArgs>? CollectParserErrorListeners;

    public TokenProviderService(DocumentManagerService documentManagerService)
    {
        _documentManagerService = documentManagerService;

        _documentManagerService.DataUpdated += OnDocumentContentChanged;
    }

    private void OnDocumentContentChanged(object? sender, ProviderDataUpdatedEventArgs<Document> e)
    {
        Parse(e.Data);
    }

    public ParserRuleContext Parse(DocumentUri uri, bool notify = true)
    {
        Document document = _documentManagerService.GetData(uri);
        return Parse(document, notify);
    }

    public ParserRuleContext Parse(Document document, bool notify = true)
    {
        SPSLLexer lexer = new(new AntlrInputStream(document.GetText()));
        lexer.RemoveErrorListeners();

        SPSLParser parser = new(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();

        List<IAntlrErrorListener<IToken>> tempListeners = new();
        CollectParserErrorListeners?.Invoke(this, new(document.Uri) { ErrorListeners = tempListeners });
        parser.AddErrorListener(new ProxyParserErrorListener(tempListeners));

        ParserRuleContext tree = document.Uri.Path.EndsWith(".spslm") ? parser.materialFile() : parser.shaderFile();
        SetData(document.Uri, tree, notify);

        return tree;
    }

    #region IProviderService<ParserRuleContext> Members

    public event EventHandler<ProviderDataUpdatedEventArgs<ParserRuleContext>>? DataUpdated;

    public ParserRuleContext? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out ParserRuleContext? context) ? context : null;
    }

    public void SetData(DocumentUri uri, ParserRuleContext data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data, (_, _) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
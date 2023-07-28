using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// <see cref="IProviderService{ParserRuleContext}"/> implementation that provides <see cref="ParserRuleContext"/> data.
/// </summary>
public class TokenProviderService: IProviderService<ParserRuleContext>
{
    private readonly ConcurrentDictionary<DocumentUri, ParserRuleContext> _cache = new();

    private readonly DocumentManagerService _documentManagerService;

    public event EventHandler<CollectParserErrorListenersEventArgs>? CollectParserErrorListeners;

    public TokenProviderService(DocumentManagerService documentManagerService)
    {
        _documentManagerService = documentManagerService;
        
        _documentManagerService.DocumentContentChanged += OnDocumentContentChanged;
    }

    private void OnDocumentContentChanged(object? sender, DocumentEventArgs e)
    {
        Document document = _documentManagerService.GetDocument(e.Uri);

        SPSLLexer lexer = new(new AntlrInputStream(document.GetText()));
        lexer.RemoveErrorListeners();

        SPSLParser parser = new(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();

        List<IAntlrErrorListener<IToken>> tempListeners = new();
        CollectParserErrorListeners?.Invoke(this, new(e.Uri) { ErrorListeners = tempListeners });
        parser.AddErrorListener(new ProxyParserErrorListener(tempListeners));

        ParserRuleContext tree = document.Uri.Path.EndsWith(".spslm") ? parser.materialFile() : parser.shaderFile();
        DataUpdated?.Invoke(this, new(e.Uri, tree));
    }

    #region IProviderService<ParserRuleContext> Members
    
    public event EventHandler<ProviderDataUpdatedEventArgs<ParserRuleContext>>? DataUpdated;
    
    public ParserRuleContext? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out ParserRuleContext? context) ? context : null;
    }

    public void SetData(DocumentUri uri, ParserRuleContext data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data, (_, __) => data);

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
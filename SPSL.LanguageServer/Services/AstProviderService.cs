using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language;
using SPSL.Language.AST;
using SPSL.Language.Visitors;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public class AstProviderService
{
    private readonly ConcurrentDictionary<DocumentUri, AST> _documentAst = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly List<IParserErrorListener> _errorListeners = new();

    public event EventHandler<DocumentAstEventArgs>? DocumentAstChanged;
    public event EventHandler<CollectParserErrorListenersEventArgs>? CollectParserErrorListeners;

    public AstProviderService(DocumentManagerService documentManagerService)
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
        tempListeners.AddRange(_errorListeners);
        parser.AddErrorListener(new ProxyParserErrorListener(tempListeners));

        ParserRuleContext tree = document.Uri.Path.EndsWith(".spslm") ? parser.materialFile() : parser.shaderFile();

        ASTVisitor visitor = new(e.Uri.ToString());
        AST ast = visitor.Visit(tree);

        _documentAst.AddOrUpdate(document.Uri, ast, (_, _) => ast);
        DocumentAstChanged?.Invoke(this, new(document.Uri, ast));
    }

    public void AddParserErrorListener(IParserErrorListener errorListener)
    {
        _errorListeners.Add(errorListener);
    }

    public void RemoveParserErrorListener(IParserErrorListener errorListener)
    {
        _errorListeners.Remove(errorListener);
    }
}
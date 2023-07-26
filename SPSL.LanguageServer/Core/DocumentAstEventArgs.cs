using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.AST;

namespace SPSL.LanguageServer.Core;

public class DocumentAstEventArgs : EventArgs
{
    public DocumentUri Uri { get; init; }

    public AST Ast { get; init; }

    public DocumentAstEventArgs(DocumentUri uri, AST ast)
    {
        Uri = uri;
        Ast = ast;
    }
}
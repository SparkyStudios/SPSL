using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace SPSL.LanguageServer.Core;

public class CollectParserErrorListenersEventArgs : EventArgs
{
    public DocumentUri Uri { get; init; }

    public List<IAntlrErrorListener<IToken>> ErrorListeners { get; init; } = new();

    public CollectParserErrorListenersEventArgs(DocumentUri uri)
    {
        Uri = uri;
    }
}
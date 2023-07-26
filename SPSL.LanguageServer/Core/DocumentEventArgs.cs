using OmniSharp.Extensions.LanguageServer.Protocol;

namespace SPSL.LanguageServer.Core;

public class DocumentEventArgs : EventArgs
{
    public DocumentUri Uri { get; init; }
    
    public CancellationToken CancellationToken { get; init; }

    public DocumentEventArgs(DocumentUri uri)
    {
        Uri = uri;
    }
}
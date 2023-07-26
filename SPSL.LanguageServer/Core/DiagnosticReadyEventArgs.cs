using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace SPSL.LanguageServer.Core;

public class DiagnosticReadyEventArgs : EventArgs
{
    public DocumentUri Uri { get; init; }

    public Container<Diagnostic> Diagnostics { get; init; }

    public DiagnosticReadyEventArgs(DocumentUri uri, Container<Diagnostic> diagnostics)
    {
        Uri = uri;
        Diagnostics = diagnostics;
    }
}
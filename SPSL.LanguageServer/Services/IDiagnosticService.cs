using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public interface IDiagnosticService
{
    public event EventHandler<DiagnosticReadyEventArgs>? DiagnosticReady;

    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken);
}
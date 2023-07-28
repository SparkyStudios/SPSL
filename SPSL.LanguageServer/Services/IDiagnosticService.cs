﻿using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public interface IDiagnosticService : IProviderService<Container<Diagnostic>>
{
    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken);
}
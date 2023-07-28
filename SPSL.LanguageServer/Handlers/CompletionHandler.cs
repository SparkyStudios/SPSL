using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Listeners;
using SPSL.Language.Symbols;
using SPSL.LanguageServer.Services;

namespace SPSL.LanguageServer.Handlers;

public class CompletionHandler : ICompletionHandler, ICompletionResolveHandler
{
    public Guid Id => Guid.Parse("710c73d8-440c-4f6e-a93f-85b958c9b6df");

    private readonly SymbolProviderService _symbolProviderService;
    
    private readonly DocumentSelector _documentSelector = new
    (
        new DocumentFilter
        {
            Pattern = "**/*.spsl*",
            Scheme = "file",
            Language = "spsl"
        }
    );

    public CompletionHandler(SymbolProviderService symbolProviderService)
    {
        _symbolProviderService = symbolProviderService;
        
        _symbolProviderService.DataUpdated += SymbolProviderServiceOnDataUpdated;
    }

    private void SymbolProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<SymbolTable> e)
    {
        
    }

    public CompletionRegistrationOptions GetRegistrationOptions(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = _documentSelector,
        };
    }

    public Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult
        (
            new CompletionList
            (
                new CompletionItem
                {
                    Kind = CompletionItemKind.Keyword,
                    Label = "class",
                }
            )
        );
    }

    public void SetCapability(CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
      
    }
}

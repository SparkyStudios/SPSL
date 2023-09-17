using System.Collections.Concurrent;
using Antlr4.Runtime;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.Language.Analysis.Common;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Analysis.Visitors;
using SPSL.LanguageServer.Core;
using static SPSL.Language.Analysis.Visitors.StaticAnalyzerVisitor;
using Diagnostic = OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace SPSL.LanguageServer.Services;

public class StaticAnalyzerService : IDiagnosticService
{
    public const string SourceName = "spsl";

    private readonly ConcurrentDictionary<DocumentUri, List<Diagnostic>> _cache = new();

    private readonly DocumentManagerService _documentManagerService;
    private readonly ConfigurationService _configurationService;
    private readonly TokenProviderService _tokenProviderService;
    private readonly WorkspaceService _workspaceService;

    public StaticAnalyzerService
    (
        DocumentManagerService documentManagerService,
        ConfigurationService configurationService,
        TokenProviderService tokenProviderService,
        WorkspaceService workspaceService
    )
    {
        _documentManagerService = documentManagerService;
        _configurationService = configurationService;
        _tokenProviderService = tokenProviderService;
        _workspaceService = workspaceService;

        _documentManagerService.DataUpdated += DocumentManagerServiceOnDataUpdated;
    }

    private void DocumentManagerServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<Document> e)
    {
        // Do not perform static analysis if the workspace is not initialized.
        if (!_workspaceService.IsInitialized) return;

        Document document = e.Data;
        StaticAnalyzerVisitor visitor = new
        (
            e.Uri.ToString(),
            _workspaceService.WorkspaceAst,
            _workspaceService.WorkspaceSymbolTable
        );

        ParserRuleContext? tree = _tokenProviderService.GetData(e.Uri);
        if (tree == null)
            return;

        var diagnostics = _cache.GetOrAdd(e.Uri, new List<Diagnostic>());
        diagnostics.Clear();

        diagnostics.AddRange(tree.Accept(visitor).Select(d =>
        {
            Range range = new()
            {
                Start = document.PositionAt(d.Start),
                End = document.PositionAt(d.End + 1)
            };

            return new Diagnostic
            {
                Code = d.Code,
                Severity = d.Severity switch
                {
                    Severity.Error => DiagnosticSeverity.Error,
                    Severity.Warning => DiagnosticSeverity.Warning,
                    Severity.Information => DiagnosticSeverity.Information,
                    Severity.Hint => DiagnosticSeverity.Hint,
                    _ => DiagnosticSeverity.Hint
                },
                Range = range,
                Source = "spsl",
                Message = d.Message,
            };
        }));

        SetData(e.Uri, diagnostics);
    }

    private void SymbolProviderServiceOnDataUpdated(object? sender, ProviderDataUpdatedEventArgs<SymbolTable> e)
    {
        Document document = _documentManagerService.GetData(e.Uri);

        var diagnostics = _cache.GetOrAdd(e.Uri, new List<Diagnostic>());
        diagnostics.Clear();

        CheckShaderConstructorNames(document, e.Data, diagnostics);
        CheckConflictingVariables(document, e.Data, diagnostics);

        SetData(e.Uri, diagnostics);
    }

    private void CheckShaderConstructorNames(Document document, SymbolTable table, ICollection<Diagnostic> diagnostics)
    {
        if (table.Type == SymbolType.Shader)
        {
            Symbol? constructor = table.Symbols.FirstOrDefault(symbol => symbol.Type == SymbolType.Constructor);
            if (constructor == null)
                return;

            if (constructor.Name == table.Name) return;

            Range range = new()
            {
                Start = document.PositionAt(constructor.Start),
                End = document.PositionAt(constructor.End + 1)
            };

            diagnostics.Add
            (
                new()
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = range,
                    Message = "Mismatching constructor name.",
                    Source = SourceName,
                    Code = MismatchConstructorNameErrorCode,
                    RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                        ? new
                        (
                            new DiagnosticRelatedInformation
                            {
                                Message = $"The constructor doesn't match the shader name: {table.Name}",
                                Location = new()
                                {
                                    Uri = document.Uri,
                                    Range = range
                                }
                            }
                        )
                        : new Container<DiagnosticRelatedInformation>()
                }
            );
        }
        else
        {
            foreach (Symbol symbol in table.Symbols)
                if (symbol is SymbolTable t)
                    CheckShaderConstructorNames(document, t, diagnostics);
        }
    }

    private void CheckConflictingVariables(Document document, SymbolTable table, ICollection<Diagnostic> diagnostics)
    {
        table.Symbols.Where(symbol => symbol.Key.Contains('@')).ToList().ForEach(symbol =>
        {
            Range range = new()
            {
                Start = document.PositionAt(symbol.Start),
                End = document.PositionAt(symbol.End + 1)
            };

            diagnostics.Add
            (
                new()
                {
                    Severity = DiagnosticSeverity.Error,
                    Range = range,
                    Message = symbol.Type switch
                    {
                        SymbolType.Function => "A function with this signature already exists.",
                        _ => "Conflicting identifier."
                    },
                    Source = SourceName,
                    Code = ConflictingIdentifierNameErrorCode,
                    RelatedInformation = _configurationService.HasDiagnosticRelatedInformationCapability
                        ? new
                        (
                            new DiagnosticRelatedInformation
                            {
                                Message = symbol.Type switch
                                {
                                    SymbolType.Function =>
                                        "You cannot declare two functions with the exact same signature, or only with different return values.",
                                    _ => "The name given to this identifier is already used."
                                },
                                Location = new()
                                {
                                    Uri = document.Uri,
                                    Range = range
                                }
                            }
                        )
                        : new Container<DiagnosticRelatedInformation>()
                }
            );
        });

        foreach (Symbol symbol in table.Symbols)
            if (symbol is SymbolTable t)
                CheckConflictingVariables(document, t, diagnostics);
    }

    #region IDiagnosticService Implementation

    public IEnumerable<Diagnostic> Diagnose(DocumentUri uri, CancellationToken cancellationToken)
    {
        if (!_documentManagerService.HasDocument(uri))
            return ArraySegment<Diagnostic>.Empty;

        return _cache.TryGetValue(uri, out var diagnostics) ? diagnostics : ArraySegment<Diagnostic>.Empty;
    }

    #endregion

    #region IProviderService<Container<Diagnostic>> Implementation

    public event EventHandler<ProviderDataUpdatedEventArgs<Container<Diagnostic>>>? DataUpdated;

    public Container<Diagnostic>? GetData(DocumentUri uri)
    {
        return _cache.TryGetValue(uri, out var diagnostics) ? new(diagnostics) : null;
    }

    public void SetData(DocumentUri uri, Container<Diagnostic> data, bool notify = true)
    {
        _cache.AddOrUpdate(uri, data.ToList(), (_, _) => data.ToList());

        if (!notify) return;
        DataUpdated?.Invoke(this, new(uri, data));
    }

    #endregion
}
using Antlr4.Runtime.Tree;
using SPSL.Language.Analysis.Common;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;

namespace SPSL.Language.Analysis.Visitors;

public class StaticAnalyzerVisitor : SPSLBaseVisitor<List<Diagnostic>>
{
    public const string StaticAnalyzerErrorCode = "E0000";
    public const string MismatchConstructorNameErrorCode = "E0001";
    public const string ConflictingIdentifierNameErrorCode = "E0002";
    public const string ReassignedConstVariableErrorCode = "E0003";
    public const string UndefinedVariableUsageErrorCode = "E0004";

    public const string StaticAnalyzerWarningCode = "W0000";
    public const string ImportedNamespaceNotFoundWarningCode = "W0001";
    public const string ImportedShaderFragmentNotFoundWarningCode = "W0002";

    private readonly string _fileSource;
    private readonly Ast _ast;

    protected override List<Diagnostic> DefaultResult { get; } = new();

    public StaticAnalyzerVisitor(string fileSource, Ast ast)
    {
        _fileSource = fileSource;
        _ast = ast;
    }

    protected override List<Diagnostic> AggregateResult(List<Diagnostic> aggregate, List<Diagnostic> nextResult)
    {
        return aggregate.Concat(nextResult).ToList();
    }

    public override List<Diagnostic> VisitUseNamespaceDirective(SPSLParser.UseNamespaceDirectiveContext context)
    {
        ITerminalNode[] tokens = context.Name.IDENTIFIER();
        return CheckNamespaceExists(tokens);
    }

    public override List<Diagnostic> VisitUseFragmentDirective(SPSLParser.UseFragmentDirectiveContext context)
    {
        ITerminalNode[] tokens = context.Name.IDENTIFIER();
        var diagnostics = CheckNamespaceExists(tokens[..^1]);
        
        if (diagnostics.Count > 0)
            return diagnostics;
        
        string[] nsSplit = tokens[..^1].Select(item => item.Symbol.Text).ToArray();
        string fragmentName = tokens[^1].Symbol.Text;

        Namespace ns = _ast.GetNamespace(string.Join(Namespace.Separator, nsSplit))!;
        INamespaceChild? fragment = ns.Children.FirstOrDefault(item => item.Name.Value == fragmentName);

        if (fragment == null)
        {
            return new()
            {
                new()
                {
                    Code = ImportedShaderFragmentNotFoundWarningCode,
                    Severity = Severity.Warning,
                    Message = "The imported shader fragment is not found in the workspace.",
                    Source = _fileSource,
                    Start = tokens[^1].Symbol.StartIndex,
                    End = tokens[^1].Symbol.StopIndex
                }
            };
        }

        return DefaultResult;
    }

    private List<Diagnostic> CheckNamespaceExists(ITerminalNode[] tokens)
    {
        string[] nsSplit = tokens.Select(item => item.Symbol.Text).ToArray();

        var missCount = 0;
        while (true)
        {
            string nsString = nsSplit.Length > missCount
                ? string.Join(Namespace.Separator, nsSplit[..^missCount])
                : string.Empty;

            if (nsString.Length == 0)
                break;

            Namespace? ns = _ast.GetNamespace(nsString);
            if (ns == null)
            {
                missCount++;
                continue;
            }

            break;
        }

        if (missCount > 0)
        {
            return new()
            {
                new()
                {
                    Code = ImportedNamespaceNotFoundWarningCode,
                    Severity = Severity.Warning,
                    Message = "The imported namespace is not found in the workspace.",
                    Source = _fileSource,
                    Start = tokens[missCount - 1].Symbol.StartIndex,
                    End = tokens[^1].Symbol.StopIndex
                }
            };
        }

        return DefaultResult;
    }
}
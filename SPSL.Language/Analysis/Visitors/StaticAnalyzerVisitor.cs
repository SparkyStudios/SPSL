using Antlr4.Runtime;
using SPSL.Language.Analysis.Common;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Parsing.Utils;
using SPSL.Language.Parsing.Visitors;
using SPSL.Language.Utils;
using static SPSL.Language.Core.SPSLParser;

namespace SPSL.Language.Analysis.Visitors;

public class StaticAnalyzerVisitor : SPSLBaseVisitor<List<Diagnostic>>
{
    public const string StaticAnalyzerErrorCode = "E0000";
    public const string MismatchConstructorNameErrorCode = "E0001";
    public const string ConflictingIdentifierNameErrorCode = "E0002";
    public const string ReassignedConstVariableErrorCode = "E0003";
    public const string UndefinedVariableUsageErrorCode = "E0004";
    public const string IncompleteInterfaceImplementationErrorCode = "E0005";
    public const string SymbolNotFoundErrorCode = "E0006";
    public const string MissingNamespaceImportErrorCode = "E0007";
    public const string InvalidSymbolUsageErrorCode = "E0008";
    public const string LastReturnStatementInConstructorErrorCode = "E0009";
    public const string InvalidReturnStatementInConstructorErrorCode = "E0010";
    public const string ReturnedValueInVoidFunctionErrorCode = "E0011";
    public const string InvalidPermutationUsedInPermuteBlockErrorCode = "E0012";

    public const string StaticAnalyzerWarningCode = "W0000";
    public const string ImportedNamespaceNotFoundWarningCode = "W0001";
    public const string ImportedShaderFragmentNotFoundWarningCode = "W0002";

    private readonly string _fileSource;

    private readonly Ast _ast;
    private readonly SymbolTable _symbolTable;

    private NamespacedReference _currentNamespace = NamespacedReference.Null;
    private List<NamespacedReference> _importedNamespaces = new();

    protected override List<Diagnostic> DefaultResult { get; } = new();

    public StaticAnalyzerVisitor(string fileSource, Ast ast, SymbolTable symbolTable)
    {
        _fileSource = fileSource;

        _ast = ast;
        _symbolTable = symbolTable;
    }

    protected override List<Diagnostic> AggregateResult(List<Diagnostic> aggregate, List<Diagnostic> nextResult)
    {
        return aggregate.Concat(nextResult).ToList();
    }

    public override List<Diagnostic> VisitNamespaceDefinition(NamespaceDefinitionContext context)
    {
        _currentNamespace = context.Name.ToNamespaceReference(_fileSource);

        return VisitChildren(context);
    }

    public override List<Diagnostic> VisitUseNamespaceDirective(UseNamespaceDirectiveContext context)
    {
        List<Diagnostic> diagnostics = new();
        CheckNamespaceExists(context.Name._Name.ToArray(), diagnostics);

        if (diagnostics.Count == 0)
            _importedNamespaces.Add(context.Name.ToNamespaceReference(_fileSource));

        diagnostics.AddRange(VisitChildren(context));

        return diagnostics;
    }

    public override List<Diagnostic> VisitUseFragmentDirective(UseFragmentDirectiveContext context)
    {
        IToken[] tokens = context.Name._Name.ToArray();
        List<Diagnostic> diagnostics = new();
        CheckNamespaceExists(tokens[..^1], diagnostics);

        if (diagnostics.Count > 0)
            return diagnostics;

        string[] nsSplit = tokens[..^1].Select(item => item.Text).ToArray();
        string fragmentName = tokens[^1].Text;

        Namespace? ns = _ast.GetNamespace(string.Join(Namespace.Separator, nsSplit));
        if (ns == null)
        {
            return new()
            {
                new()
                {
                    Code = ImportedShaderFragmentNotFoundWarningCode,
                    Severity = Severity.Warning,
                    Message = "The imported shader fragment is not found in the workspace.",
                    Source = _fileSource,
                    Start = tokens[^1].StartIndex,
                    End = tokens[^1].StopIndex
                }
            };
        }

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
                    Start = tokens[^1].StartIndex,
                    End = tokens[^1].StopIndex
                }
            };
        }

        diagnostics.AddRange(VisitChildren(context));

        return DefaultResult;
    }

    public override List<Diagnostic> VisitGenericShaderDefinition(GenericShaderDefinitionContext context)
    {
        if (_ast.GetNamespace(_currentNamespace)?.GetChild(context.Name.Text) is not Shader shader)
            return DefaultResult;

        List<Diagnostic> diagnostics = new();

        CheckInterfaceImplementation
        (
            shader,
            context.Interfaces?.namespacedTypeName()?.Select(i => i.ToNamespaceReference(_fileSource)).ToArray() ??
            Array.Empty<NamespacedReference>(),
            diagnostics
        );

        diagnostics.AddRange(VisitChildren(context));

        return diagnostics;
    }

    public override List<Diagnostic> VisitFunctionBody(FunctionBodyContext context)
    {
        var body = context.ToStatementBlock(_fileSource);
        List<Diagnostic> diagnostics = new();

        switch (context.Parent)
        {
            case ShaderConstructorFunctionContext:
            {
                if (body.Children.LastOrDefault() is ReturnStatement returnStatement)
                    diagnostics.Add
                    (
                        new()
                        {
                            Code = LastReturnStatementInConstructorErrorCode,
                            Severity = Severity.Error,
                            Message = "The last return statement in a shader constructor is not allowed.",
                            Source = _fileSource,
                            Start = returnStatement.Start,
                            End = returnStatement.End
                        }
                    );

                CheckReturnStatementInBlock(body, CheckConstructorReturnStatementInBlock);

                break;
            }

            case FunctionContext function:
            {
                IDataType type = function.Head.Type.Accept(new DataTypeVisitor(_fileSource));
                switch (type)
                {
                    case PrimitiveDataType { Type: PrimitiveDataTypeKind.Void }:
                    {
                        CheckReturnStatementInBlock(body, CheckVoidFunctionReturnStatementInBlock);
                        break;
                    }
                }

                break;
            }
        }

        diagnostics.AddRange(VisitChildren(context));

        return diagnostics;

        void CheckReturnStatementInBlock(StatementBlock statementBlock, Action<StatementBlock> checker)
        {
            checker(statementBlock);

            foreach (StatementBlock statement in statementBlock.Children.OfType<StatementBlock>())
                checker(statement);

            foreach (IfStatement statement in statementBlock.Children.OfType<IfStatement>())
            {
                checker(statement.If.Block);

                foreach (IfStatement.IfStatementConditionBlock conditionBlock in statement.Elif)
                    checker(conditionBlock.Block);

                if (statement.Else != null)
                    checker(statement.Else);
            }

            foreach (WhileStatement statement in statementBlock.Children.OfType<WhileStatement>())
                checker(statement.Block);
        }

        void CheckConstructorReturnStatementInBlock(StatementBlock statementBlock)
        {
            if (statementBlock.Children.Any(item => item is ReturnStatement
                {
                    Expression: BasicExpression { Identifier.Value: not "out_streams" }
                }))
                diagnostics.Add
                (
                    new()
                    {
                        Code = InvalidReturnStatementInConstructorErrorCode,
                        Severity = Severity.Error,
                        Message =
                            "A shader constructor may only return the out_streams variable of the current stage.",
                        Source = _fileSource,
                        Start = statementBlock.Start,
                        End = statementBlock.End
                    }
                );
        }

        void CheckVoidFunctionReturnStatementInBlock(StatementBlock statementBlock)
        {
            if (statementBlock.Children.Any(item => item is ReturnStatement
                {
                    Expression: not null
                }))
                diagnostics.Add
                (
                    new()
                    {
                        Code = ReturnedValueInVoidFunctionErrorCode,
                        Severity = Severity.Error,
                        Message =
                            "A void function cannot return a value.",
                        Source = _fileSource,
                        Start = statementBlock.Start,
                        End = statementBlock.End
                    }
                );
        }
    }

    public override List<Diagnostic> VisitPermuteStatement(PermuteStatementContext context)
    {
        List<Diagnostic> diagnostics = new();

        if (context.Accept(new StatementVisitor(_fileSource)) is not PermuteStatement statement)
            return diagnostics;

        if (statement.Condition is not BinaryOperationExpression condition)
            return diagnostics;

        if (condition.Left is not BasicExpression variable)
            return diagnostics;

        SymbolTable scope = _symbolTable.FindEnclosingScope(_fileSource, variable.Start);
        Symbol? permutation = scope.Resolve(variable.Identifier.Value);
        if (permutation == null)
            diagnostics.Add
            (
                new()
                {
                    Code = InvalidPermutationUsedInPermuteBlockErrorCode,
                    Severity = Severity.Error,
                    Message = "The symbol used is not a permutation variable.",
                    Source = _fileSource,
                    Start = variable.Start,
                    End = variable.End
                }
            );

        diagnostics.AddRange(VisitChildren(context));

        return diagnostics;
    }

    private void CheckNamespaceExists(IReadOnlyList<IToken> tokens, ICollection<Diagnostic> diagnostics)
    {
        string[] nsSplit = tokens.Select(item => item.Text).ToArray();

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
            diagnostics.Add
            (
                new()
                {
                    Code = ImportedNamespaceNotFoundWarningCode,
                    Severity = Severity.Warning,
                    Message = "The imported namespace is not found in the workspace.",
                    Source = _fileSource,
                    Start = tokens[missCount - 1].StartIndex,
                    End = tokens[^1].StopIndex
                }
            );
        }
    }

    private void CheckNamespaceChildExist
    (
        NamespacedReference reference,
        ICollection<Diagnostic> diagnostics,
        out INamespaceChild? child
    )
    {
        child = null;

        if (reference.NamespaceName.Length > 0 &&
            !_importedNamespaces.Exists(item => item.Name == reference.NamespaceName))
        {
            diagnostics.Add
            (
                new()
                {
                    Code = MissingNamespaceImportErrorCode,
                    Severity = Severity.Error,
                    Message = "The namespace of this symbol is not imported.",
                    Source = _fileSource,
                    Start = reference.Start,
                    End = reference.End
                }
            );

            return;
        }

        if ((child = _ast.GetNamespace(_currentNamespace)?.GetChild(reference.Name)) != null)
            return;

        foreach (NamespacedReference ns in _importedNamespaces)
        {
            child = _ast.GetNamespace(ns)?.GetChild(reference.Name);
            if (child != null)
                return;
        }

        diagnostics.Add
        (
            new()
            {
                Code = SymbolNotFoundErrorCode,
                Severity = Severity.Error,
                Message = "Cannot find a symbol with that name.",
                Source = _fileSource,
                Start = reference.Start,
                End = reference.End
            }
        );
    }

    private void CheckInterfaceImplementation
    (
        Shader shader,
        IReadOnlyCollection<NamespacedReference> interfaces,
        ICollection<Diagnostic> diagnostics
    )
    {
        if (interfaces.Count == 0)
            return;

        var shaderMethodsHashCodes = shader.Methods.Select(m => m.Function.GetSemanticHashCode()).ToList();

        DetectMissingMethods(interfaces);
        return;

        void DetectMissingMethods(IEnumerable<NamespacedReference> references)
        {
            foreach (NamespacedReference reference in references)
            {
                DetectMissingMethodsCore(reference, reference);
            }
        }

        void DetectMissingMethodsCore(NamespacedReference parent, NamespacedReference reference)
        {
            CheckNamespaceChildExist(reference, diagnostics, out INamespaceChild? child);

            if (child is not Interface and not null)
            {
                diagnostics.Add
                (
                    new()
                    {
                        Code = InvalidSymbolUsageErrorCode,
                        Severity = Severity.Error,
                        Message = "The used symbol is not valid for the current context.",
                        Source = _fileSource,
                        Start = reference.Start,
                        End = reference.End
                    }
                );

                return;
            }

            if (child == null)
                return;

            var @interface = (Interface)child;

            List<string> misses = (from method in @interface.Methods
                let hashCode = method.GetSemanticHashCode()
                where !shaderMethodsHashCodes.Contains(hashCode)
                select DeclarationString.From(method)).ToList();

            if (misses.Count > 0)
            {
                diagnostics.Add
                (
                    new()
                    {
                        Code = IncompleteInterfaceImplementationErrorCode,
                        Severity = Severity.Error,
                        Message =
                            $"The interface `{@interface.Name.Value}` is not fully implemented. Missing {string.Join(", ", misses.Select(s => $"`{s}`"))}.",
                        Source = _fileSource,
                        Start = parent.Start,
                        End = parent.End
                    }
                );
            }

            foreach (NamespacedReference extended in @interface.ExtendedInterfaces)
            {
                DetectMissingMethodsCore(parent, extended);
            }
        }
    }
}
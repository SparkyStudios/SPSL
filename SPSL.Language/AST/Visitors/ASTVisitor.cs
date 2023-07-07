using Antlr4.Runtime.Tree;
using SPSL.Language.Utils;
using static System.String;

namespace SPSL.Language.AST.Visitors;

public class ASTVisitor : SPSLBaseVisitor<AST>
{
    private Namespace _currentNamespace = new(Empty);

    public readonly OrderedSet<string> Imports = new();

    protected override AST DefaultResult => new();

    internal static NamespacedReference ParseNamespacedTypeName(SPSLParser.NamespacedTypeNameContext? context)
    {
        return context is not null ? new NamespacedReference(context.GetText()) : NamespacedReference.Null;
    }

    internal static ShaderFunction ParseShaderFunction(SPSLParser.ShaderFunctionContext context)
    {
        var function = new ShaderFunction(ParseFunction(context.Function))
        {
            IsOverride = context.IsOverride
        };

        foreach (SPSLParser.AnnotationContext annotation in context.annotation())
        {
            function.Annotations.Add
            (
                new Annotation
                {
                    Name = annotation.Name.Text,
                    Arguments = new(annotation.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
                }
            );
        }

        return function;
    }

    internal static Function ParseFunction(SPSLParser.FunctionContext context)
    {
        return new Function(ParseFunctionHead(context.Head), ParseFunctionBody(context.Body));
    }

    internal static FunctionHead ParseFunctionHead(SPSLParser.FunctionHeadContext context)
    {
        return new FunctionHead
        (
            context.Type.Accept(new DataTypeVisitor()),
            context.Name.Text,
            ParseFunctionSignature(context.Signature)
        );
    }

    internal static FunctionSignature ParseFunctionSignature(SPSLParser.FunctionSignatureContext context)
    {
        FunctionSignature signature = new();

        if (context.Arguments == null) return signature;

        foreach (SPSLParser.ArgDefContext arg in context.Arguments.argDef())
        {
            signature.AddParameter
            (
                new FunctionArgument
                (
                    arg.Flow?.Text switch
                    {
                        "in" => DataFlow.In,
                        "out" => DataFlow.Out,
                        "inout" => DataFlow.InOut,
                        "const" => DataFlow.Const,
                        _ => DataFlow.Unspecified
                    },
                    arg.Type.Accept(new DataTypeVisitor()),
                    arg.Name.Text
                )
            );
        }

        return signature;
    }

    internal static StatementBlock ParseFunctionBody(SPSLParser.FunctionBodyContext context)
    {
        OrderedSet<IStatement> statements = new();
        StatementVisitor statementVisitor = new();

        foreach (SPSLParser.StayControlFlowStatementContext statement in context.stayControlFlowStatement())
            statements.Add(statement.Accept(statementVisitor)!);

        if (context.ReturnStatement != null)
            statements.Add(context.ReturnStatement.Accept(statementVisitor)!);

        return new StatementBlock(statements);
    }

    internal static GlobalVariable ParseGlobalVariable(SPSLParser.GlobalVariableContext context)
    {
        return new
        (
            context.IsStatic,
            context.Type.Accept(new DataTypeVisitor()),
            context.Definition.Expression.Accept(new ExpressionVisitor())!
        )
        {
            Name = context.Definition.Identifier.Identifier.Text,
        };
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableContext context)
    {
        if (context.permutationVariableBool() != null)
            return ParsePermutationVariable(context.permutationVariableBool());

        if (context.permutationVariableEnum() != null)
            return ParsePermutationVariable(context.permutationVariableEnum());

        if (context.permutationVariableInteger() != null)
            return ParsePermutationVariable(context.permutationVariableInteger());

        throw new ArgumentException("The provided context is not valid.");
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableBoolContext context)
    {
        return new PermutationVariable
        (
            PermutationVariable.VariableType.Bool,
            new BoolLiteral(bool.Parse(context.Value.Text))
        )
        {
            Name = context.Identifier.GetText(),
        };
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableEnumContext context)
    {
        return new PermutationVariable
        (
            PermutationVariable.VariableType.Enum,
            context.Value.Accept(new ExpressionVisitor())!
        )
        {
            EnumerationValues = context.IDENTIFIER().Select(id => id.GetText()).ToArray(),
            Name = context.Identifier.GetText(),
        };
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableIntegerContext context)
    {
        return new PermutationVariable
        (
            PermutationVariable.VariableType.Integer,
            new IntegerLiteral(int.Parse(context.Value.Text))
        )
        {
            Name = context.Identifier.GetText(),
        };
    }

    internal static TypeProperty ParseBufferComponent(SPSLParser.BufferComponentContext context)
        => new TypeProperty(context.Type.Accept(new DataTypeVisitor()), context.Name.Text);


    protected TypeKind GetTypeKind(string type)
    {
        return type.ToLower() switch
        {
            "struct" => TypeKind.Struct,
            "enum" => TypeKind.Enum,
            _ => throw new ArgumentException("The given SPSL type is not recognized."),
        };
    }

    protected override AST AggregateResult(AST aggregate, AST nextResult)
    {
        return aggregate.Merge(nextResult);
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, AST currentResult)
    {
        switch (node)
        {
            // Not parsed
            case SPSLParser.DirectiveContext _:
            // Not parsed
            case SPSLParser.NamespacedTypeNameContext _:
            // Not parsed
            case SPSLParser.EnumContext _:
            // Not parsed
            case SPSLParser.StructContext _:
            // Not parsed
            case SPSLParser.InterfaceDefinitionContext _:
            // Not parsed
            case SPSLParser.ShaderFragmentDefinitionContext _:
                return false;

            default:
                return base.ShouldVisitNextChild(node, currentResult);
        }
    }

    public override AST VisitFile(SPSLParser.FileContext context)
    {
        // TODO: Parse directives

        return VisitChildren(context);
    }

    public override AST VisitNamespaceDefinition(SPSLParser.NamespaceDefinitionContext context)
    {
        var ns = context.Name.GetText().Split('/');

        Namespace? current = null;
        for (int i = 0, l = ns.Length; i < l; i++)
        {
            Namespace n = new(ns[i]);
            current?.AddChild(n);
            current = n;
        }

        _currentNamespace = current!;

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitUseDirective(SPSLParser.UseDirectiveContext context)
    {
        Imports.Add(context.namespacedTypeName().GetText());
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitPermutationVariableBool(SPSLParser.PermutationVariableBoolContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitPermutationVariableEnum(SPSLParser.PermutationVariableEnumContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitPermutationVariableInteger(SPSLParser.PermutationVariableIntegerContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitGlobalVariable(SPSLParser.GlobalVariableContext context)
    {
        _currentNamespace.AddChild(ParseGlobalVariable(context));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitStruct(SPSLParser.StructContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor())!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitEnum(SPSLParser.EnumContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor())!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitInterface(SPSLParser.InterfaceContext context)
    {
        var tName = context.Definition.Name.Text;
        Interface @interface = new(tName);

        if (context.Definition.ExtendedInterfaces != null)
        {
            foreach (SPSLParser.NamespacedTypeNameContext nsd in context.Definition.ExtendedInterfaces
                         .namespacedTypeName())
                @interface.AddExtendedInterface(nsd.GetText());
        }

        // Register type members
        foreach (SPSLParser.FunctionHeadContext member in context.functionHead())
        {
            FunctionHead functionHead = ParseFunctionHead(member);

            @interface.AddFunctionHead
            (
                functionHead
            );
        }

        _currentNamespace.AddChild(@interface);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitShaderFragment(SPSLParser.ShaderFragmentContext context)
    {
        // --- Definition

        var fName = context.Definition.Name.Text;
        ShaderFragment fragment = new(fName)
        {
            ExtendedShaderFragment = ParseNamespacedTypeName(context.Definition.ExtendedFragment)
        };

        if (context.Definition.ExtendedInterfaces != null)
            foreach (SPSLParser.NamespacedTypeNameContext nsd in context.Definition.ExtendedInterfaces
                         .namespacedTypeName())
                fragment.Extends(nsd.GetText());

        // --- Use Directives

        foreach (SPSLParser.UseDirectiveContext use in context.useDirective())
            fragment.Uses(ParseNamespacedTypeName(use.Name));

        // --- Global variables

        foreach (SPSLParser.GlobalVariableContext variable in context.globalVariable())
            fragment.AddGlobalVariable(ParseGlobalVariable(variable));

        // --- Permutation variables

        foreach (SPSLParser.PermutationVariableContext variable in context.permutationVariable())
            fragment.AddPermutationVariable(ParsePermutationVariable(variable));

        // --- Shader Members

        foreach (SPSLParser.ShaderMemberContext memberContext in context.shaderMember())
            fragment.AddShaderMember(memberContext.Accept(new ShaderMemberVisitor())!);

        // --- Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            fragment.AddFunction(ParseShaderFunction(function));

        _currentNamespace.AddChild(fragment);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitShader(SPSLParser.ShaderContext context)
    {
        _currentNamespace.AddChild(context.Accept(new ShaderVisitor())!);

        return DefaultResult.AddNamespace(_currentNamespace);
    }
}
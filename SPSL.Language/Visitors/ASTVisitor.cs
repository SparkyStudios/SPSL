using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;
using static System.String;

namespace SPSL.Language.Visitors;

public class ASTVisitor : SPSLBaseVisitor<AST.AST>
{
    private Namespace _currentNamespace = new(Empty);

    public readonly OrderedSet<string> Imports = new();

    protected override AST.AST DefaultResult => new();

    internal static NamespacedReference ParseNamespacedTypeName(SPSLParser.NamespacedTypeNameContext? context)
    {
        return context is not null ? new NamespacedReference(context.GetText()) : NamespacedReference.Null;
    }

    internal static ShaderFunction ParseShaderFunction(SPSLParser.ShaderFunctionContext context)
    {
        if (context is SPSLParser.BasicShaderFunctionContext f)
            return (ASTVisitor.ParseShaderFunction(f));
        if (context is SPSLParser.ShaderConstructorFunctionContext c)
            return (ASTVisitor.ParseShaderFunction(c));

        throw new ArgumentException("Unsupported shader function.");
    }

    internal static ShaderFunction ParseShaderFunction(SPSLParser.BasicShaderFunctionContext context)
    {
        var function = new ShaderFunction(ParseFunction(context.Function))
        {
            IsOverride = context.IsOverride,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation()));
        return function;
    }

    internal static ShaderFunction ParseShaderFunction(SPSLParser.ShaderConstructorFunctionContext context)
    {
        var function = new ShaderFunction
        (
            new
            (
                new
                (
                    new PrimitiveDataType(PrimitiveDataTypeKind.Void),
                    context.Name.Text,
                    new()
                ),
                ParseFunctionBody(context.Body)
            )
        )
        {
            IsOverride = false,
            IsConstructor = true,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation()));
        return function;
    }

    internal static Function ParseFunction(SPSLParser.FunctionContext context)
    {
        return new(ParseFunctionHead(context.Head), ParseFunctionBody(context.Body))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    internal static FunctionHead ParseFunctionHead(SPSLParser.FunctionHeadContext context)
    {
        return new
        (
            context.Type.Accept(new DataTypeVisitor()),
            context.Name.Text,
            ParseFunctionSignature(context.Signature)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    internal static FunctionSignature ParseFunctionSignature(SPSLParser.FunctionSignatureContext context)
    {
        FunctionSignature signature = new()
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };

        if (context.Arguments == null) return signature;

        foreach (SPSLParser.ArgDefContext arg in context.Arguments.argDef())
        {
            signature.AddParameter
            (
                new
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
                {
                    Start = arg.Start.StartIndex,
                    End = arg.Stop.StopIndex
                }
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

        return new(statements)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
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
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
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
        return new
        (
            PermutationVariable.VariableType.Bool,
            new BoolLiteral(bool.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex
            }
        )
        {
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableEnumContext context)
    {
        return new
        (
            PermutationVariable.VariableType.Enum,
            context.Value.Accept(new ExpressionVisitor())!
        )
        {
            EnumerationValues = context.IDENTIFIER().Select(id => id.GetText()).ToArray(),
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    internal static PermutationVariable ParsePermutationVariable(SPSLParser.PermutationVariableIntegerContext context)
    {
        return new
        (
            PermutationVariable.VariableType.Integer,
            new IntegerLiteral(int.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex
            }
        )
        {
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    internal static TypeProperty ParseBufferComponent(SPSLParser.BufferComponentContext context) =>
        new(context.Type.Accept(new DataTypeVisitor()), context.Name.Text)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };


    protected TypeKind GetTypeKind(string type)
    {
        return type.ToLower() switch
        {
            "struct" => TypeKind.Struct,
            "enum" => TypeKind.Enum,
            _ => throw new ArgumentException("The given SPSL type is not recognized."),
        };
    }

    protected override AST.AST AggregateResult(AST.AST aggregate, AST.AST nextResult)
    {
        return aggregate.Merge(nextResult);
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, AST.AST currentResult)
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

    public override AST.AST VisitShaderFile(SPSLParser.ShaderFileContext context)
    {
        // TODO: Parse directives

        return VisitChildren(context);
    }

    public override AST.AST VisitMaterialFile(SPSLParser.MaterialFileContext context)
    {
        // TODO: Parse directives

        return VisitChildren(context);
    }

    public override AST.AST VisitNamespaceDefinition(SPSLParser.NamespaceDefinitionContext context)
    {
        var ns = context.Name.GetText().Split("::");

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

    public override AST.AST VisitMaterial(SPSLParser.MaterialContext context)
    {
        _currentNamespace.AddChild(context.Accept(new MaterialVisitor())!);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitUseDirective(SPSLParser.UseDirectiveContext context)
    {
        Imports.Add(context.namespacedTypeName().GetText());
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableBool(SPSLParser.PermutationVariableBoolContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableEnum(SPSLParser.PermutationVariableEnumContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableInteger(SPSLParser.PermutationVariableIntegerContext context)
    {
        _currentNamespace.AddChild(ParsePermutationVariable(context));

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitStruct(SPSLParser.StructContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor())!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitEnum(SPSLParser.EnumContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor())!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitInterface(SPSLParser.InterfaceContext context)
    {
        var tName = context.Definition.Name.Text;
        Interface @interface = new(tName)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };

        if (context.Definition.ExtendedInterfaces != null)
        {
            foreach (var nsd in context.Definition.ExtendedInterfaces.namespacedTypeName())
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

    public override AST.AST VisitShaderFragment(SPSLParser.ShaderFragmentContext context)
    {
        // --- Definition

        var fName = context.Definition.Name.Text;
        ShaderFragment fragment = new(fName)
        {
            ExtendedShaderFragment = ParseNamespacedTypeName(context.Definition.ExtendedFragment),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };

        if (context.Definition.ExtendedInterfaces != null)
            foreach (SPSLParser.NamespacedTypeNameContext nsd in context.Definition.ExtendedInterfaces
                         .namespacedTypeName())
                fragment.Extends(nsd.GetText());

        // --- Use Directives

        foreach (SPSLParser.UseDirectiveContext use in context.useDirective())
            fragment.Uses(ParseNamespacedTypeName(use.Name));

        // --- Permutation variables

        foreach (SPSLParser.PermutationVariableContext variable in context.permutationVariable())
        {
            var permutation = ParsePermutationVariable(variable);
            permutation.Parent = _currentNamespace;
            fragment.AddPermutationVariable(permutation);
        }

        // --- Shader Members

        foreach (SPSLParser.ShaderMemberContext memberContext in context.shaderMember())
        {
            var member = memberContext.Accept(new ShaderMemberVisitor())!;

            if (member is INamespaceChild child)
                child.Parent = _currentNamespace;

            fragment.AddShaderMember(member);
        }

        // --- Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            fragment.AddFunction(ParseShaderFunction(function));

        _currentNamespace.AddChild(fragment);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitShader(SPSLParser.ShaderContext context)
    {
        _currentNamespace.AddChild(context.Accept(new ShaderVisitor())!);

        return DefaultResult.AddNamespace(_currentNamespace);
    }
}
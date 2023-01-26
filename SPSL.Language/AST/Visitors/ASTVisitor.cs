using Antlr4.Runtime.Tree;
using SPSL.Language.Utils;
using static System.String;

namespace SPSL.Language.AST.Visitors;

public class ASTVisitor : SPSLBaseVisitor<AST>
{
    private Namespace _currentNamespace = new(Empty);

    protected override AST DefaultResult => new();

    private static NamespacedReference ParseNamespacedTypeName(SPSLParser.NamespacedTypeNameContext? context)
    {
        return context is not null ? new NamespacedReference(context.GetText()) : NamespacedReference.Null;
    }

    private static ShaderFunction ParseShaderFunction(SPSLParser.ShaderFunctionContext context)
    {
        return new ShaderFunction(ParseFunction(context.Function))
        {
            IsOverride = context.IsOverride
        };
    }

    private static Function ParseFunction(SPSLParser.FunctionContext context)
    {
        return new Function(ParseFunctionHead(context.Head), ParseFunctionBody(context.Body));
    }

    private static FunctionHead ParseFunctionHead(SPSLParser.FunctionHeadContext context)
    {
        return new FunctionHead
        (
            context.Type.Accept(new DataTypeVisitor()),
            context.Name.Text,
            ParseFunctionSignature(context.Signature)
        );
    }

    private static FunctionSignature ParseFunctionSignature(SPSLParser.FunctionSignatureContext context)
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
                        _ => DataFlow.Unspecified
                    },
                    arg.Type.Accept(new DataTypeVisitor()),
                    arg.Name.Text
                )
            );
        }

        return signature;
    }

    private static StatementBlock ParseFunctionBody(SPSLParser.FunctionBodyContext context)
    {
        OrderedSet<IStatement> statements = new();
        StatementVisitor statementVisitor = new();

        foreach (SPSLParser.StayControlFlowStatementContext statement in context.stayControlFlowStatement())
            statements.Add(statement.Accept(statementVisitor)!);

        if (context.ReturnStatement != null)
            statements.Add(context.ReturnStatement.Accept(statementVisitor)!);

        return new StatementBlock(statements);
    }

    protected TypeKind GetTypeKind(string type)
    {
        return type.ToLower() switch
        {
            "struct" => TypeKind.Struct,
            "enum" => TypeKind.Enum,
            _ => throw new ArgumentException("The given SPSL type is not recognized."),
        };
    }

    protected ShaderType GetShaderType(string type)
    {
        return type.ToLower() switch
        {
            "vertex" => ShaderType.Vertex,
            "fragment" => ShaderType.Pixel,
            "pixel" => ShaderType.Pixel,
            "geometry" => ShaderType.Geometry,
            "hull" => ShaderType.Hull,
            "domain" => ShaderType.Domain,
            "compute" => ShaderType.Compute,
            _ => throw new ArgumentException("The given SPSL shader type is not recognized."),
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
            case SPSLParser.UseDirectiveContext _:
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

    public override AST VisitChildren(IRuleNode node)
    {
        AST result = DefaultResult;

        for (int i = 0, l = node.ChildCount; i < l && ShouldVisitNextChild(node, result); ++i)
        {
            AST nextResult = node.GetChild(i).Accept(this);
            result = AggregateResult(result, nextResult);
        }

        return result;
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
        return base.VisitUseDirective(context);
    }

    public override AST VisitStruct(SPSLParser.StructContext context)
    {
        TypeKind tKind = TypeKind.Struct;
        var tName = context.Definition.Name.Text;
        Type type = new(tKind, tName)
        {
            ExtendedType = ParseNamespacedTypeName(context.Definition.ExtendedType),
        };

        // Register struct members
        foreach (SPSLParser.BlockComponentContext member in context.blockComponent())
        {
            var mType = member.Type.Accept(new DataTypeVisitor());
            var mName = member.Name.Text;
            type.AddMember(new TypeMember(mType, mName));
        }

        _currentNamespace.AddChild(type);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitEnum(SPSLParser.EnumContext context)
    {
        TypeKind tKind = TypeKind.Enum;
        var tName = context.Definition.Name.Text;
        Type type = new(tKind, tName);

        uint lastValue = 0;

        // Register enum members
        foreach (SPSLParser.EnumBlockComponentContext member in context.enumBlockComponent())
        {
            var mName = member.Name.Text;
            IConstantExpression? value = (member.Value?.Accept(new ExpressionVisitor())) as IConstantExpression;

            if (value is not null && value is ILiteral constant)
            {
                lastValue = constant switch
                {
                    IntegerLiteral integer => (uint)integer.Value,
                    UnsignedIntegerLiteral integer => integer.Value,
                    _ => throw new NotSupportedException("The specified enum value is not supported. Only integer values are accepted.")
                };
            }

            type.AddMember(new TypeMember(new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger), mName) { Initializer = new UnsignedIntegerLiteral(lastValue) });
            lastValue++;
        }

        _currentNamespace.AddChild(type);

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
        var fName = context.Definition.Name.Text;
        ShaderFragment fragment = new(fName)
        {
            ExtendedShaderFragment = ParseNamespacedTypeName(context.Definition.ExtendedFragment)
        };

        if (context.Definition.ExtendedInterfaces != null)
            foreach (SPSLParser.NamespacedTypeNameContext nsd in context.Definition.ExtendedInterfaces
                         .namespacedTypeName())
                fragment.AddExtendedInterface(nsd.GetText());

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            fragment.AddFunction(ParseShaderFunction(function));

        _currentNamespace.AddChild(fragment);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST VisitShader(SPSLParser.ShaderContext context)
    {
        // --- Shader Definition

        ShaderType sType = context.Definition.Type switch
        {
            null => ShaderType.Unspecified,
            _ => GetShaderType(context.Definition.Type.Text)
        };
        var sName = context.Definition.Name.Text;

        Shader shader = new(sType, sName)
        {
            IsAbstract = context.Definition.IsAbstract,
            ExtendedShader = ParseNamespacedTypeName(context.Definition.ExtendedShader)
        };

        if (context.Definition.Interfaces is not null)
            foreach (SPSLParser.NamespacedTypeNameContext @interface in context.Definition.Interfaces.namespacedTypeName())
                shader.Implements(ParseNamespacedTypeName(@interface));


        // --- Use Directives

        foreach (SPSLParser.UseDirectiveContext shaderFragment in context.useDirective())
            shader.Uses(ParseNamespacedTypeName(shaderFragment.Name));

        // --- Shader Members

        // --- Shader Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
        {
            shader.Children.Add(ParseShaderFunction(function));
        }

        _currentNamespace.AddChild(shader);

        return DefaultResult.AddNamespace(_currentNamespace);
    }
}
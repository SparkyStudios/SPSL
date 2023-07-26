using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;
using static System.String;

namespace SPSL.Language.Visitors;

public class ASTVisitor : SPSLBaseVisitor<AST.AST>
{
    private Namespace _currentNamespace = new(Empty);
    private readonly string _fileSource;

    public readonly OrderedSet<string> Imports = new();

    protected override AST.AST DefaultResult => new();

    internal static NamespacedReference ParseNamespacedTypeName(SPSLParser.NamespacedTypeNameContext? context)
    {
        return context is not null ? new(context.GetText()) : NamespacedReference.Null;
    }

    public ASTVisitor(string fileSource)
    {
        _fileSource = fileSource;
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
        _currentNamespace.AddChild(context.Accept(new MaterialVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitUseDirective(SPSLParser.UseDirectiveContext context)
    {
        Imports.Add(context.namespacedTypeName().GetText());
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableBool(SPSLParser.PermutationVariableBoolContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableEnum(SPSLParser.PermutationVariableEnumContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitPermutationVariableInteger(SPSLParser.PermutationVariableIntegerContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitStruct(SPSLParser.StructContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitEnum(SPSLParser.EnumContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitInterface(SPSLParser.InterfaceContext context)
    {
        var tName = context.Definition.Name.Text;
        Interface @interface = new(tName)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        if (context.Definition.ExtendedInterfaces != null)
        {
            foreach (var nsd in context.Definition.ExtendedInterfaces.namespacedTypeName())
                @interface.AddExtendedInterface(nsd.GetText());
        }

        // Register type members
        foreach (SPSLParser.FunctionHeadContext member in context.functionHead())
            @interface.AddFunctionHead(member.ToFunctionHead(_fileSource));

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
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        if (context.Definition.ExtendedInterfaces != null)
            foreach (var nsd in context.Definition.ExtendedInterfaces.namespacedTypeName())
                fragment.Extends(nsd.GetText());

        // --- Use Directives

        foreach (SPSLParser.UseDirectiveContext use in context.useDirective())
            fragment.Uses(ParseNamespacedTypeName(use.Name));

        // --- Permutation variables

        foreach (SPSLParser.PermutationVariableContext variable in context.permutationVariable())
        {
            PermutationVariable permutation = variable.ToPermutationVariable(_fileSource);
            permutation.Parent = _currentNamespace;

            fragment.AddPermutationVariable(permutation);
        }

        // --- Shader Members

        foreach (SPSLParser.ShaderMemberContext memberContext in context.shaderMember())
        {
            IShaderMember member = memberContext.Accept(new ShaderMemberVisitor(_fileSource))!;

            if (member is INamespaceChild child)
            {
                child.Parent = _currentNamespace;
                child.Source = _fileSource;
            }

            fragment.AddShaderMember(member);
        }

        // --- Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            fragment.AddFunction(function.Accept(new ShaderFunctionVisitor(_fileSource))!);

        _currentNamespace.AddChild(fragment);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override AST.AST VisitShader(SPSLParser.ShaderContext context)
    {
        _currentNamespace.AddChild(context.Accept(new ShaderVisitor(_fileSource))!);

        return DefaultResult.AddNamespace(_currentNamespace);
    }
}
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;
using static SPSL.Language.SPSLParser;

namespace SPSL.Language.Visitors;

public class AstVisitor : SPSLBaseVisitor<Ast>
{
    private Namespace _currentNamespace = new(new());
    private readonly string _fileSource;

    public readonly OrderedSet<NamespacedReference> Imports = new();

    protected override Ast DefaultResult => new();

    public AstVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override Ast AggregateResult(Ast aggregate, Ast nextResult)
    {
        return aggregate.Merge(nextResult);
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, Ast currentResult)
    {
        return true;
    }

    public override Ast VisitShaderFile([NotNull] ShaderFileContext context)
    {
        // TODO: Parse directives

        return VisitChildren(context);
    }

    public override Ast VisitMaterialFile([NotNull] MaterialFileContext context)
    {
        // TODO: Parse directives

        return VisitChildren(context);
    }

    public override Ast VisitNamespaceDefinition([NotNull] NamespaceDefinitionContext context)
    {
        var ns = context.Name.IDENTIFIER().Select(x => x.Symbol.ToIdentifier(_fileSource)).ToArray();

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

    public override Ast VisitMaterial([NotNull] MaterialContext context)
    {
        _currentNamespace.AddChild(context.Accept(new MaterialVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitUseNamespaceDirective([NotNull] UseNamespaceDirectiveContext context)
    {
        Imports.Add(context.namespacedTypeName().ToNamespaceReference(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitPermutationVariableBool([NotNull] PermutationVariableBoolContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitPermutationVariableEnum([NotNull] PermutationVariableEnumContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitPermutationVariableInteger([NotNull] PermutationVariableIntegerContext context)
    {
        _currentNamespace.AddChild(context.ToPermutationVariable(_fileSource));
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitStruct([NotNull] StructContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitEnum([NotNull] EnumContext context)
    {
        _currentNamespace.AddChild(context.Accept(new TypeVisitor(_fileSource))!);
        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitInterface([NotNull] InterfaceContext context)
    {
        var tName = context.Definition.Name.ToIdentifier(_fileSource);
        Interface @interface = new(tName)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        if (context.Definition.ExtendedInterfaces != null)
        {
            foreach (var nsd in context.Definition.ExtendedInterfaces.namespacedTypeName())
                @interface.Extends(nsd.ToNamespaceReference(_fileSource));
        }

        // Register type members
        foreach (FunctionHeadContext member in context.functionHead())
            @interface.Implements(member.ToFunctionHead(_fileSource));

        _currentNamespace.AddChild(@interface);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitShaderFragment([NotNull] ShaderFragmentContext context)
    {
        // --- Definition

        var fName = context.Definition.Name.ToIdentifier(_fileSource);
        ShaderFragment fragment = new(fName)
        {
            ExtendedShaderFragment = context.Definition.ExtendedFragment.ToNamespaceReference(_fileSource),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        if (context.Definition.ExtendedInterfaces != null)
            foreach (NamespacedTypeNameContext nsd in context.Definition.ExtendedInterfaces
                         .namespacedTypeName())
                fragment.Extends(nsd.ToNamespaceReference(_fileSource));

        // --- Permutation variables

        foreach (PermutationVariableContext variable in context.permutationVariable())
        {
            var permutation = variable.ToPermutationVariable(_fileSource);
            permutation.ParentNamespace = _currentNamespace;

            fragment.AddPermutationVariable(permutation);
        }

        // --- Shader Members

        foreach (ShaderMemberContext memberContext in context.shaderMember())
        {
            if (memberContext.useFragmentDirective() is not null)
            {
                fragment.Uses(memberContext.useFragmentDirective().Name.ToNamespaceReference(_fileSource));
                continue;
            }

            IShaderMember member = memberContext.Accept(new ShaderMemberVisitor(_fileSource))!;

            if (member is INamespaceChild child)
                child.ParentNamespace = _currentNamespace;

            fragment.AddShaderMember(member);
        }

        _currentNamespace.AddChild(fragment);

        return DefaultResult.AddNamespace(_currentNamespace);
    }

    public override Ast VisitShader([NotNull] ShaderContext context)
    {
        _currentNamespace.AddChild(context.Accept(new ShaderVisitor(_fileSource))!);

        return DefaultResult.AddNamespace(_currentNamespace);
    }
}
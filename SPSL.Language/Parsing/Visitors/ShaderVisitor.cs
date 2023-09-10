using Antlr4.Runtime.Misc;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Parsing.Utils;
using static SPSL.Language.Core.SPSLParser;

namespace SPSL.Language.Parsing.Visitors;

public class ShaderVisitor : SPSLBaseVisitor<Shader?>
{
    private readonly string _fileSource;

    public ShaderVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override Shader? DefaultResult => null;

    public override Shader VisitGenericShaderDefinition([NotNull] GenericShaderDefinitionContext context)
    {
        ShaderStage sStage = context.Type switch
        {
            null => ShaderStage.Unspecified,
            _ => context.Type.ToShaderStage()
        };
        var sName = context.Name.ToIdentifier(_fileSource);

        Shader shader = new(sStage, sName)
        {
            IsAbstract = context.IsAbstract,
            ExtendedShader = context.ExtendedShader.ToNamespaceReference(_fileSource),
            Start = ((ShaderContext)context.Parent).Start.StartIndex,
            End = ((ShaderContext)context.Parent).Stop.StopIndex,
            Source = _fileSource,
            Documentation = ((ShaderContext)context.Parent).Documentation.ToDocumentation()
        };

        if (context.Interfaces is not null)
            foreach (var @interface in context.Interfaces.namespacedTypeName())
                shader.Implements(@interface.ToNamespaceReference(_fileSource));

        return shader;
    }

    public override Shader VisitComputeShaderDefinition([NotNull] ComputeShaderDefinitionContext context)
    {
        var sName = context.Name.ToIdentifier(_fileSource);
        Shader.ComputeShaderParams @params = new()
        {
            ThreadCountX = uint.Parse(context.ThreadCountX.Text),
            ThreadCountY = uint.Parse(context.ThreadCountY.Text),
            ThreadCountZ = uint.Parse(context.ThreadCountZ.Text)
        };

        Shader shader = new(sName, @params)
        {
            IsAbstract = false,
            ExtendedShader = context.ExtendedShader.ToNamespaceReference(_fileSource),
            Start = ((ShaderContext)context.Parent).Start.StartIndex,
            End = ((ShaderContext)context.Parent).Stop.StopIndex,
            Source = _fileSource
        };

        if (context.Interfaces is not null)
            foreach (NamespacedTypeNameContext @interface in context.Interfaces.namespacedTypeName())
                shader.Implements(@interface.ToNamespaceReference(_fileSource));

        return shader;
    }

    public override Shader VisitShader([NotNull] ShaderContext context)
    {
        // --- Shader Definition

        Shader shader = context.Definition.Accept(this)!;

        // --- Shader Members

        foreach (ShaderMemberContext member in context.shaderMember())
        {
            if (member.useFragmentDirective() is not null)
            {
                shader.Uses(member.useFragmentDirective().Name.ToNamespaceReference(_fileSource));
                continue;
            }

            shader.Children.Add(member.Accept(new ShaderMemberVisitor(_fileSource))!);
        }

        return shader;
    }
}
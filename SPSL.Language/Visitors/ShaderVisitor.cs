using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class ShaderVisitor : SPSLBaseVisitor<Shader?>
{
    private readonly string _fileSource;

    public ShaderVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override Shader? DefaultResult => null;

    internal static ShaderStage GetShaderStage(string type)
    {
        return type.ToLower() switch
        {
            "vertex" => ShaderStage.Vertex,
            "fragment" => ShaderStage.Pixel,
            "pixel" => ShaderStage.Pixel,
            "geometry" => ShaderStage.Geometry,
            "hull" => ShaderStage.Hull,
            "domain" => ShaderStage.Domain,
            "compute" => ShaderStage.Compute,
            _ => throw new ArgumentException("The given SPSL shader type is not recognized."),
        };
    }

    public override Shader VisitGenericShaderDefinition(SPSLParser.GenericShaderDefinitionContext context)
    {
        ShaderStage sStage = context.Type switch
        {
            null => ShaderStage.Unspecified,
            _ => GetShaderStage(context.Type.Text)
        };
        var sName = context.Name?.Text ?? "UnknownShader";

        Shader shader = new(sStage, sName)
        {
            IsAbstract = context.IsAbstract,
            ExtendedShader = ASTVisitor.ParseNamespacedTypeName(context.ExtendedShader),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        if (context.Interfaces is not null)
            foreach (var @interface in context.Interfaces.namespacedTypeName())
                shader.Implements(ASTVisitor.ParseNamespacedTypeName(@interface));

        return shader;
    }

    public override Shader VisitComputeShaderDefinition(SPSLParser.ComputeShaderDefinitionContext context)
    {
        var sName = context.Name.Text;
        Shader.ComputeShaderParams @params = new()
        {
            ThreadCountX = uint.Parse(context.ThreadCountX.Text),
            ThreadCountY = uint.Parse(context.ThreadCountY.Text),
            ThreadCountZ = uint.Parse(context.ThreadCountZ.Text)
        };

        Shader shader = new(sName, @params)
        {
            IsAbstract = false,
            ExtendedShader = ASTVisitor.ParseNamespacedTypeName(context.ExtendedShader),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        if (context.Interfaces is not null)
            foreach (var @interface in context.Interfaces.namespacedTypeName())
                shader.Implements(ASTVisitor.ParseNamespacedTypeName(@interface));

        return shader;
    }

    public override Shader VisitShader(SPSLParser.ShaderContext context)
    {
        // --- Shader Definition

        Shader shader = context.Definition.Accept(this)!;
        shader.Start = context.Start.StartIndex;
        shader.End = context.Stop.StopIndex;

        // --- Use Directives

        foreach (SPSLParser.UseFragmentDirectiveContext use in context.useFragmentDirective())
            shader.Uses(ASTVisitor.ParseNamespacedTypeName(use.Name));

        // --- Shader Members

        foreach (SPSLParser.ShaderMemberContext member in context.shaderMember())
            shader.Children.Add(member.Accept(new ShaderMemberVisitor(_fileSource))!);

        // --- Shader Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            shader.Children.Add(function.Accept(new ShaderFunctionVisitor(_fileSource))!);

        return shader;
    }
}
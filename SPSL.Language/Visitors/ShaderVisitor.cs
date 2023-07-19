using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class ShaderVisitor : SPSLBaseVisitor<Shader?>
{
    protected override Shader? DefaultResult => null;

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

    public override Shader VisitGenericShaderDefinition(SPSLParser.GenericShaderDefinitionContext context)
    {
        ShaderType sType = context.Type switch
        {
            null => ShaderType.Unspecified,
            _ => GetShaderType(context.Type.Text)
        };
        var sName = context.Name.Text;

        Shader shader = new(sType, sName)
        {
            IsAbstract = context.IsAbstract,
            ExtendedShader = ASTVisitor.ParseNamespacedTypeName(context.ExtendedShader)
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
            ExtendedShader = ASTVisitor.ParseNamespacedTypeName(context.ExtendedShader)
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

        // --- Use Directives

        foreach (SPSLParser.UseDirectiveContext use in context.useDirective())
            shader.Uses(ASTVisitor.ParseNamespacedTypeName(use.Name));

        // --- Shader Members

        foreach (SPSLParser.ShaderMemberContext member in context.shaderMember())
            shader.Children.Add(member.Accept(new ShaderMemberVisitor())!);

        // --- Shader Functions

        foreach (SPSLParser.ShaderFunctionContext function in context.shaderFunction())
            shader.Children.Add(ASTVisitor.ParseShaderFunction(function));

        return shader;
    }
}
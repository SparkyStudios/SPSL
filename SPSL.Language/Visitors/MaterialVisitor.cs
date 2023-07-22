using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class MaterialVisitor : SPSLBaseVisitor<Material?>
{
    protected override Material? DefaultResult => null;

    public override Material VisitMaterial(SPSLParser.MaterialContext context)
    {
        Material material = new(context.Definition.Name.Text)
        {
            IsAbstract = context.Definition.IsAbstract,
            ExtendedMaterial = ASTVisitor.ParseNamespacedTypeName(context.Definition.ExtendedMaterial)
        };

        foreach (SPSLParser.MaterialMemberContext member in context.materialMember())
            material.Children.Add(member.Accept(new MaterialMemberVisitor())!);

        return material;
    }
}

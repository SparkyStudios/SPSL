using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class MaterialVisitor : SPSLBaseVisitor<Material?>
{
    protected override Material? DefaultResult => null;

    private readonly string _fileSource;

    public MaterialVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    public override Material VisitMaterial(SPSLParser.MaterialContext context)
    {
        Material material = new(context.Definition.Name.Text)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            IsAbstract = context.Definition.IsAbstract,
            ExtendedMaterial = ASTVisitor.ParseNamespacedTypeName(context.Definition.ExtendedMaterial),
            Source = _fileSource
        };

        foreach (SPSLParser.MaterialMemberContext member in context.materialMember())
            material.Children.Add(member.Accept(new MaterialMemberVisitor(_fileSource))!);

        return material;
    }
}
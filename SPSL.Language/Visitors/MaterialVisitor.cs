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
        Material material = new(context.Definition.Name.ToIdentifier(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            IsAbstract = context.Definition.IsAbstract,
            ExtendedMaterial = context.Definition.ExtendedMaterial.ToNamespaceReference(_fileSource),
            Source = _fileSource
        };

        material.Children.AddRange
        (
            context.materialMember()
                .Select(m => m.Accept(new MaterialMemberVisitor(_fileSource)))
                .Where(m => m is not null)
                .Cast<IMaterialMember>()
        );

        return material;
    }
}
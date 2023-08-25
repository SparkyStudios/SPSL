using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Collects a set of <see cref="MaterialParameter"/>s into one group.
/// </summary>
public class MaterialParameterGroup : IAnnotated, IBlock, IMaterialMember
{
    #region Properties

    /// <summary>
    /// Whether the material parameters group is partial. Partial groups are
    /// merged together from start to the end of the inheritance hierarchy.
    /// </summary>
    public bool IsPartial { get; set; }

    #endregion

    #region Constructors

    public MaterialParameterGroup(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    public MaterialParameterGroup(Identifier name, IEnumerable<MaterialParameter> parameters)
    {
        Name = name;
        Children.AddRange(parameters);

        Name.Parent = this;
        foreach (IBlockChild parameter in Children)
            parameter.Parent = this;
    }

    #endregion

    #region IAnnotated Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The parameter name.
    /// </summary>
    public Identifier Name { get; set; }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Name.ResolveNode(source, offset) ??
               Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               Children.FirstOrDefault(c => c.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
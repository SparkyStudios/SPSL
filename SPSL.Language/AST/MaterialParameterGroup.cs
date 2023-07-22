using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Collects a set of <see cref="MaterialParameter"/>s into one group.
/// </summary>
public class MaterialParameterGroup : IAnnotable, IBlock, IMaterialMember
{
    #region Properties

    /// <summary>
    /// Whether the material parameters group is partial. Partial groups are
    /// merged together from start to the end of the inheritance hierarchy.
    /// </summary>
    public bool IsPartial { get; set; } = false;

    #endregion

    #region Constructors

    public MaterialParameterGroup(string name)
    {
        Name = name;
    }

    public MaterialParameterGroup(string name, IEnumerable<MaterialParameter> parameters)
    {
        Name = name;
        Children.AddRange(parameters);
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The parameter name.
    /// </summary>
    public string Name { get; set; }

    #endregion
}

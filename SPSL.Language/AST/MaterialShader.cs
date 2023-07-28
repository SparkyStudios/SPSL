using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// References a <see cref="Shader"/> for usage inside a <see cref="Material"/>.
/// </summary>
public class MaterialShader : IBlock, IMaterialMember
{
    #region Properties

    /// <summary>
    /// The fully-qualified name of the referenced <see cref="Shader"/>.
    /// </summary>
    public NamespacedReference ReferencedShader { get; set; } = NamespacedReference.Null;

    /// <summary>
    /// The shader stage.
    /// </summary>
    public ShaderStage Stage { get; set; }

    /// <summary>
    /// The list of imported <see cref="ShaderFragment"/>s.
    /// </summary>
    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    #endregion

    #region Constructors

    public MaterialShader(string name)
    {
        Name = name;
    }

    #endregion

    #region Methods

    public void Uses(NamespacedReference name)
    {
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        ImportedShaderFragments.AddRange(names);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The material shader name.
    /// </summary>
    public string Name { get; set; }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
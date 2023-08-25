using SPSL.Language.Core;
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
    public NamespacedReference ReferencedShader { get; init; } = NamespacedReference.Null;

    /// <summary>
    /// The shader stage.
    /// </summary>
    public ShaderStage Stage { get; init; }

    /// <summary>
    /// The list of imported <see cref="ShaderFragment"/>s.
    /// </summary>
    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    #endregion

    #region Constructors

    public MaterialShader(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    #endregion

    #region Methods

    public void Uses(NamespacedReference name)
    {
        name.Parent = this;
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (NamespacedReference name in names)
            Uses(name);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The material shader name.
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
        return ReferencedShader.ResolveNode(source, offset) ??
               ImportedShaderFragments.FirstOrDefault(x => x.ResolveNode(source, offset) != null)
                   ?.ResolveNode(source, offset) ??
               Name.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
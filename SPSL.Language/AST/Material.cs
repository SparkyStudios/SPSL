using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL material.
/// </summary>
public class Material : IBlock, INamespaceChild
{
    #region Properties

    /// <summary>
    /// Whether this material is abstract.
    /// </summary>
    public bool IsAbstract { get; init; }

    /// <summary>
    /// A full-qualified reference to the extended material, if any.
    /// </summary>
    public NamespacedReference ExtendedMaterial { get; init; } = NamespacedReference.Null;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Material"/> class.
    /// </summary>
    /// <param name="name">The name of the material.</param>
    public Material(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    #endregion

    #region IBlock Implementation

    /// <inheritdoc cref="IBlock.Children"/>
    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region INamespaceChild Implementation

    /// <inheritdoc cref="INamespaceChild.ParentNamespace"/>
    public Namespace? ParentNamespace { get; set; }

    /// <inheritdoc cref="INamespaceChild.Name"/>
    public Identifier Name { get; set; }

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return ExtendedMaterial.ResolveNode(source, offset) ??
               Children.FirstOrDefault(c => c.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
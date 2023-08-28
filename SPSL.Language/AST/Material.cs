using SPSL.Language.Utils;

namespace SPSL.Language.AST;

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

    public Material(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Language.AST.Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? ParentNamespace { get; set; }

    /// <summary>
    /// The namespace's name.
    /// </summary>
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
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return ExtendedMaterial.ResolveNode(source, offset) ??
               Children.FirstOrDefault(c => c.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
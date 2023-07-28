using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Material : INamespaceChild, IBlock
{
    #region Properties

    /// <summary>
    /// Whether this material is abstract.
    /// </summary>
    public bool IsAbstract { get; set; } = false;

    /// <summary>
    /// A full-qualified reference to the extended material, if any.
    /// </summary>
    public NamespacedReference ExtendedMaterial { get; set; } = NamespacedReference.Null;

    #endregion

    #region Constructors

    public Material(string name)
    {
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
    public Namespace? Parent { get; set; }

    /// <summary>
    /// The namespace's name.
    /// </summary>
    public string Name { get; set; }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
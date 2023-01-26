using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Shader : INamespaceChild, IBlock
{
    #region Properties

    public bool IsAbstract { get; set; }

    public ShaderType Type { get; }

    public NamespacedReference ExtendedShader { get; set; } = NamespacedReference.Null;

    public HashSet<NamespacedReference> Interfaces { get; } = new();

    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    #endregion

    #region Constructors

    public Shader(ShaderType type, string name)
    {
        Type = type;
        Name = name;
    }

    #endregion

    #region Public Methods

    public void Implements(NamespacedReference name)
    {
        Interfaces.Add(name);
    }

    public void Implements(IEnumerable<NamespacedReference> names)
    {
        foreach (var name in names)
            Implements(name);
    }

    public void Uses(NamespacedReference name)
    {
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (var name in names)
            Uses(name);
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
}
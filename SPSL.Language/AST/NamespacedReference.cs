namespace SPSL.Language.AST;

/// <summary>
/// Reference to a namespace child, using its name.
/// </summary>
public class NamespacedReference : IEquatable<NamespacedReference>
{
    #region Fields

    /// <summary>
    /// The null reference.
    /// </summary>
    public static readonly NamespacedReference Null = new(string.Empty);

    #endregion

    #region Properties

    /// <summary>
    /// The reference name. It can be appended with the namespace
    /// path in which the referenced child exists.
    /// </summary>
    /// <example>A/Namespace/Path/To/ReferenceName</example>
    public string Name { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="NamespacedReference"/>.
    /// </summary>
    /// <param name="name">The reference name.</param>
    public NamespacedReference(string name)
    {
        Name = name;
    }

    #endregion

    #region Method

    private INamespaceChild? ResolveInternal(Namespace ns, string root)
    {
        if (root == string.Empty)
            return null;

        var pos = root.IndexOf('/');

        if (pos < 0) return ns.GetChild(root);

        var name = root[..pos];
        return name.StartsWith(ns.FullName) ? ns.GetChild(name[ns.FullName.Length..]) : null;
    }

    /// <summary>
    /// Tries to get the referenced object from the given <paramref name="ns">namespace</paramref>. If the object
    /// doesn't exist, returns <see langword="null"/>.
    /// </summary>
    /// <param name="ns">The <see cref="Namespace"/> to get the reference from.</param>
    /// <returns>The referenced object.</returns>
    public INamespaceChild? Resolve(Namespace ns)
    {
        return ResolveInternal(ns, Name);
    }

    public bool Equals(NamespacedReference? other)
    {
        return other != null && Name.Equals(other.Name);
    }

    public override string ToString()
    {
        return Name;
    }

    #endregion
}
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
    /// <example>A::Namespace::Path::To::ReferenceName</example>
    public string Name { get; }

    /// <summary>
    /// Tries to resolve the namespace name from the reference name. If the reference name
    /// was not appended with the namespace, this will return a <see cref="string.Empty"/>.
    /// </summary>
    /// <value>
    /// The resolved namespace name.
    /// </value>
    public string NamespaceName
    {
        get
        {
            var index = Name.LastIndexOf("::", StringComparison.Ordinal);
            return index == -1 ? string.Empty : Name.Substring(0, index);
        }
    }

    /// <summary>
    /// Tries to resolve the child name from the reference name. If the reference name
    /// was not appended with the namespace, this will return the same value as <see cref="Name"/>.
    /// </summary>
    public string NameWithoutNamespace => NamespaceName.Length > 0 ? Name.Substring(NamespaceName.Length + 2) : Name;

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

    #region IEquatable<NamespacedReference> Implementation

    public bool Equals(NamespacedReference? other)
    {
        return other != null && Name.Equals(other.Name);
    }

    #endregion

    #region Overrides

    public override string ToString()
    {
        return Name;
    }

    #endregion

    #region Operators

    public static bool operator ==(NamespacedReference? left, NamespacedReference? right)
    {
        return left is not null && right is not null && left.Equals(right);
    }

    public static bool operator !=(NamespacedReference? left, NamespacedReference? right)
    {
        return !(left == right);
    }

    #endregion
}
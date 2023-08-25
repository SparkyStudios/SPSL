namespace SPSL.Language.AST;

/// <summary>
/// Reference to a namespace child, using its name.
/// </summary>
public class NamespacedReference : INode, IEquatable<NamespacedReference>
{
    #region Fields

    /// <summary>
    /// The null reference.
    /// </summary>
    public static readonly NamespacedReference Null = new();

    #endregion

    #region Properties

    public Identifier[] Names { get; }

    /// <summary>
    /// The reference name. It can be appended with the namespace
    /// path in which the referenced child exists.
    /// </summary>
    /// <example>A::Namespace::Path::To::ReferenceName</example>
    public string Name => string.Join(Namespace.Separator, Names.Select(name => name.Value));

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
            int index = Name.LastIndexOf(Namespace.Separator, StringComparison.Ordinal);
            return index == -1 ? string.Empty : Name[..index];
        }
    }

    /// <summary>
    /// Tries to resolve the child name from the reference name. If the reference name
    /// was not appended with the namespace, this will return the same value as <see cref="Name"/>.
    /// </summary>
    public string NameWithoutNamespace =>
        NamespaceName.Length > 0 ? Name[(NamespaceName.Length + Namespace.SeparatorLength)..] : Name;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="NamespacedReference"/>.
    /// </summary>
    /// <param name="names">The reference name, as an array of identifier.</param>
    public NamespacedReference(params Identifier[] names)
    {
        foreach (Identifier name in names)
            name.Parent = this;

        Names = names;
    }

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
        return Names.FirstOrDefault(id => id.ResolveNode(source, offset) is not null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region IEquatable<NamespacedReference> Implementation

    public bool Equals(NamespacedReference? other)
    {
        return other != null && Name.Equals(other.Name);
    }

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((NamespacedReference)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

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
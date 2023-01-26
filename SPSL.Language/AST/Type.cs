namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL type.
/// </summary>
public class Type : INamespaceChild
{
    #region Properties

    /// <summary>
    /// The kind of type.
    /// </summary>
    /// <value>
    /// A value of the <see cref="TypeKind"/> enumeration.
    /// </value>
    public TypeKind Kind { get; set; }

    /// <summary>
    /// The collection of members in this types.
    /// </summary>
    public HashSet<TypeMember> Members { get; }

    /// <summary>
    /// The type which this one extends, if any.
    /// </summary>
    public NamespacedReference ExtendedType { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    public Type()
    {
        Name = string.Empty;
        Kind = TypeKind.Unknown;
        ExtendedType = NamespacedReference.Null;
        Members = new HashSet<TypeMember>();
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, string name, params TypeMember[] members)
        : this()
    {
        Kind = kind;
        Name = name;
        Members = new HashSet<TypeMember>(members);
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, string name, IEnumerable<TypeMember> members)
        : this()
    {
        Kind = kind;
        Name = name;
        Members = new HashSet<TypeMember>(members);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new member to this type.
    /// </summary>
    /// <param name="member">The member definition.</param>
    public void AddMember(TypeMember member)
    {
        Members.Add(member);
    }

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Language.AST.Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? Parent { get; set; }

    /// <summary>
    /// The type name.
    /// </summary>
    public string Name { get; set; }

    #endregion
}
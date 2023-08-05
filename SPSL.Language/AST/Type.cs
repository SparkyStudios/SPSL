using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL type.
/// </summary>
public class Type : INamespaceChild, IShaderMember, IMaterialMember
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
    public OrderedSet<TypeProperty> Properties { get; }

    /// <summary>
    /// The collection of functions in this types. Only available for struct types.
    /// </summary>
    public OrderedSet<TypeFunction> Functions { get; }

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
        Properties = new();
        Functions = new();
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, string name, params TypeProperty[] members)
        : this()
    {
        Kind = kind;
        Name = name;
        Properties = new(members);
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, string name, IEnumerable<TypeProperty> members)
        : this()
    {
        Kind = kind;
        Name = name;
        Properties = new(members);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new property to this type.
    /// </summary>
    /// <param name="property">The property definition.</param>
    public void AddProperty(TypeProperty property)
    {
        Properties.Add(property);
    }

    /// <summary>
    /// Adds a new function to this type. Only available for struct types.
    /// </summary>
    /// <param name="function">The function definition.</param>
    public void AddFunction(TypeFunction function)
    {
        Functions.Add(function);
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

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
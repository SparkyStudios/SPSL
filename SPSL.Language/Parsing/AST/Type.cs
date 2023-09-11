using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an SPSL type.
/// </summary>
public class Type : INamespaceChild, IShaderMember, IMaterialMember, ISemanticallyEquatable
{
    #region Properties

    /// <summary>
    /// The kind of type.
    /// </summary>
    /// <value>
    /// A value of the <see cref="TypeKind"/> enumeration.
    /// </value>
    public TypeKind Kind { get; }

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
    public NamespacedReference ExtendedType { get; init; } = NamespacedReference.Null;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, Identifier name, params TypeProperty[] members)
        : this(kind, name, (IEnumerable<TypeProperty>)members)
    {
    }

    /// <summary>
    /// Initialize a new instance of the <see cref="Type"/>.
    /// </summary>
    /// <param name="kind">The type kind.</param>
    /// <param name="name">The type name.</param>
    /// <param name="members">The collection of type members.</param>
    public Type(TypeKind kind, Identifier name, IEnumerable<TypeProperty> members)
    {
        name.Parent = this;

        Kind = kind;
        Name = name;
        Properties = new(members);
        Functions = new();

        foreach (TypeProperty member in Properties)
            member.Parent = this;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds a new property to this type.
    /// </summary>
    /// <param name="property">The property definition.</param>
    public void AddProperty(TypeProperty property)
    {
        property.Parent = this;
        Properties.Add(property);
    }

    /// <summary>
    /// Adds a new function to this type. Only available for struct types.
    /// </summary>
    /// <param name="function">The function definition.</param>
    public void AddFunction(TypeFunction function)
    {
        function.Parent = this;
        Functions.Add(function);
    }

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
        return Properties.FirstOrDefault(p => p.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               Functions.FirstOrDefault(f => f.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               ExtendedType.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset);
    }

    #endregion

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not Type other) return false;

        // Two types are semantically equal if they have the same kind and same name.
        return Kind == other.Kind && Name.SemanticallyEquals(other.Name);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Kind, Name.GetSemanticHashCode());
    }

    #endregion
}
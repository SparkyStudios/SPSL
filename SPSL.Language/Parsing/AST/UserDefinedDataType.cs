namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a data type created by the user in code.
/// </summary>
public class UserDefinedDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The reference to the user-defined type.
    /// </summary>
    public NamespacedReference Type { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UserDefinedDataType"/>.
    /// </summary>
    /// <param name="type">The data type reference.</param>
    public UserDefinedDataType(NamespacedReference type)
    {
        type.Parent = this;

        Type = type;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((UnknownDataType)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, IsArray, ArraySize);
    }

    #endregion

    #region IDataType Implementation

    /// <inheritdoc cref="IDataType.IsArray"/>
    public bool IsArray { get; init; }

    /// <inheritdoc cref="IDataType.ArraySize"/>
    public uint? ArraySize { get; init; } = null;

    #endregion

    #region ISemanticallyEquatable<IDataType> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(IDataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return other is UserDefinedDataType type && Type.Equals(type.Type) && IsArray == other.IsArray &&
               ArraySize == other.ArraySize;
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(IsArray, ArraySize);
    }

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return other is UserDefinedDataType type && Type.Equals(type.Type) && IsArray == other.IsArray &&
               ArraySize == other.ArraySize;
    }

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
        return Type.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
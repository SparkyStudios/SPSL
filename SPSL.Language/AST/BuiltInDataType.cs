using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL built-in data type.
/// </summary>
public class BuiltInDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The effective type of the built-in value.
    /// </summary>
    public BuiltInDataTypeKind Type { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PrimitiveDataType"/>.
    /// </summary>
    /// <param name="type">The data type.</param>
    public BuiltInDataType(BuiltInDataTypeKind type)
    {
        Type = type;
    }

    #endregion

    #region Overrides

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
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion

    #region ISemanticallyEquatable<IDataType> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(IDataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return other is BuiltInDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize;
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Type, IsArray, ArraySize);
    }

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return other is BuiltInDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize;
    }

    #endregion
}
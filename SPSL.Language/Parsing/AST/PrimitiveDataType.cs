using SPSL.Language.Parsing.Common;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents data type for primitive values.
/// </summary>
public class PrimitiveDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The effective type of the primitive value.
    /// </summary>
    public PrimitiveDataTypeKind Type { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PrimitiveDataType"/>.
    /// </summary>
    /// <param name="type">The data type.</param>
    public PrimitiveDataType(PrimitiveDataTypeKind type)
    {
        Type = type;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((PrimitiveDataType)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Start, End, Source);
    }

    #endregion

    #region IDataType Implementation

    /// <inheritdoc cref="IDataType.IsArray"/>
    public bool IsArray { get; set; }

    /// <inheritdoc cref="IDataType.ArraySize"/>
    public uint? ArraySize { get; set; } = null;

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
        return other is PrimitiveDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize;
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Type);
    }

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return other is PrimitiveDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize && Source == otherType.Source && Start == otherType.Start &&
               End == otherType.End;
    }

    #endregion
}
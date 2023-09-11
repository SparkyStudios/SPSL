namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a unknown data type in SPSL.
///
/// A data type is marked as Unknown when the parser was not able to recognize it.
/// There are very edge cases where this node is present in the AST.
/// </summary>
public class UnknownDataType : IDataType
{
    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
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
        return HashCode.Combine(IsArray, ArraySize);
    }

    #endregion

    #region IDataType Implementation

    /// <inheritdoc cref="IDataType.IsArray"/>
    public bool IsArray { get; init; } = false;

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

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode?)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not UnknownDataType other) return false;

        return IsArray == other.IsArray && ArraySize == other.ArraySize;
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
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

        return other is UnknownDataType otherType && IsArray == otherType.IsArray && ArraySize == otherType.ArraySize;
    }

    #endregion
}
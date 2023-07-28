namespace SPSL.Language.AST;

public class UnknownDataType : IDataType
{
    #region IDataType Implementation

    // <inheritdoc cref="IDataType.IsArray"/>
    public bool IsArray { get; set; } = false;

    /// <inheritdoc cref="IDataType.ArraySize"/>
    public uint? ArraySize { get; set; } = null;

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        return other is UnknownDataType otherType && IsArray == otherType.IsArray && ArraySize == otherType.ArraySize;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
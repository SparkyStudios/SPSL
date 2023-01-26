namespace SPSL.Language.AST;

/// <summary>
/// The list of primitive data types in SPSL.
/// </summary>
public enum PrimitiveDataTypeKind : byte
{
    Void,
    Boolean,
    Integer,
    UnsignedInteger,
    Float,
    Double,
    String,
}

/// <summary>
/// Represent data type for primitive values.
/// </summary>
public class PrimitiveDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The effective type of the primitive value.
    /// </summary>
    public PrimitiveDataTypeKind Type { get; set; }

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

    #region IDataType Implementation

    /// <inheritdoc cref="IDataType.IsArray"/>
    public bool IsArray { get; set; }

    /// <inheritdoc cref="IDataType.ArraySize"/>
    public uint? ArraySize { get; set; } = null;

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        return other is PrimitiveDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray && ArraySize == otherType.ArraySize;
    }

    #endregion
}
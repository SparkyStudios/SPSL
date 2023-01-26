namespace SPSL.Language.AST;

/// <summary>
/// Represent a data type created by the user in code.
/// </summary>
public class UserDefinedDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The reference to the user-defined type.
    /// </summary>
    public NamespacedReference Type { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UserDefinedDataType"/>.
    /// </summary>
    /// <param name="type">The data type reference.</param>
    public UserDefinedDataType(NamespacedReference type)
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
        return other is UserDefinedDataType otherType && Type.Equals(otherType.Type) && IsArray == otherType.IsArray && ArraySize == otherType.ArraySize;
    }

    #endregion
}
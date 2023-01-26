namespace SPSL.Language.AST;

/// <summary>
/// Represent any kind of data type.
/// </summary>
public interface IDataType : IEquatable<IDataType>
{
    /// <summary>
    /// Define if this data type is an array type.
    /// </summary>
    bool IsArray { get; set; }

    /// <summary>
    /// Define the size of the array if <see cref="IsArray"/> is <c>true</c>.
    /// </summary>
    uint? ArraySize { get; set; }
}
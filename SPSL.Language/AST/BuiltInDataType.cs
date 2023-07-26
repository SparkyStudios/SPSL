namespace SPSL.Language.AST;

public enum BuiltInDataTypeKind : byte
{
    Vector2b,
    Vector2f,
    Vector2i,
    Vector2ui,
    Vector3b,
    Vector3f,
    Vector3i,
    Vector3ui,
    Vector4b,
    Vector4f,
    Vector4i,
    Vector4ui,
    Matrix2f,
    Matrix3f,
    Matrix4f,
    Matrix2x3f,
    Matrix2x4f,
    Matrix3x2f,
    Matrix3x4f,
    Matrix4x2f,
    Matrix4x3f,
    Color3,
    Color4,
    Sampler,
    Texture1D,
    ArrayTexture1D,
    Texture2D,
    ArrayTexture2D,
    Texture3D,
    Cubemap,
    ArrayCubemap,
}

/// <summary>
/// Represent an SPSL built-in data type.
/// </summary>
public class BuiltInDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The effective type of the built-in value.
    /// </summary>
    public BuiltInDataTypeKind Type { get; set; }

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
        return other is BuiltInDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
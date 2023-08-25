using System.Text;
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

    public override string ToString()
    {
        StringBuilder sb = new();

        switch (Type)
        {
            case BuiltInDataTypeKind.Vector2b:
                sb.Append("vec2b");
                break;
            case BuiltInDataTypeKind.Vector2f:
                sb.Append("vec2f");
                break;
            case BuiltInDataTypeKind.Vector2i:
                sb.Append("vec2i");
                break;
            case BuiltInDataTypeKind.Vector2ui:
                sb.Append("vec2ui");
                break;
            case BuiltInDataTypeKind.Vector3b:
                sb.Append("vec3b");
                break;
            case BuiltInDataTypeKind.Vector3f:
                sb.Append("vec3f");
                break;
            case BuiltInDataTypeKind.Vector3i:
                sb.Append("vec3i");
                break;
            case BuiltInDataTypeKind.Vector3ui:
                sb.Append("vec3ui");
                break;
            case BuiltInDataTypeKind.Vector4b:
                sb.Append("vec4b");
                break;
            case BuiltInDataTypeKind.Vector4f:
                sb.Append("vec4f");
                break;
            case BuiltInDataTypeKind.Vector4i:
                sb.Append("vec4i");
                break;
            case BuiltInDataTypeKind.Vector4ui:
                sb.Append("vec4ui");
                break;
            case BuiltInDataTypeKind.Matrix2f:
                sb.Append("mat2f");
                break;
            case BuiltInDataTypeKind.Matrix3f:
                sb.Append("mat3f");
                break;
            case BuiltInDataTypeKind.Matrix4f:
                sb.Append("mat4f");
                break;
            case BuiltInDataTypeKind.Matrix2x3f:
                sb.Append("mat2x3f");
                break;
            case BuiltInDataTypeKind.Matrix2x4f:
                sb.Append("mat2x4f");
                break;
            case BuiltInDataTypeKind.Matrix3x2f:
                sb.Append("mat3x2f");
                break;
            case BuiltInDataTypeKind.Matrix3x4f:
                sb.Append("mat3x4f");
                break;
            case BuiltInDataTypeKind.Matrix4x2f:
                sb.Append("mat4x2f");
                break;
            case BuiltInDataTypeKind.Matrix4x3f:
                sb.Append("mat4x3f");
                break;
            case BuiltInDataTypeKind.Color3:
                sb.Append("color3");
                break;
            case BuiltInDataTypeKind.Color4:
                sb.Append("color4");
                break;
            case BuiltInDataTypeKind.Sampler:
                sb.Append("sampler");
                break;
            case BuiltInDataTypeKind.Texture1D:
                sb.Append("tex1d");
                break;
            case BuiltInDataTypeKind.ArrayTexture1D:
                sb.Append("arraytex1d");
                break;
            case BuiltInDataTypeKind.Texture2D:
                sb.Append("tex2d");
                break;
            case BuiltInDataTypeKind.ArrayTexture2D:
                sb.Append("arraytex2d");
                break;
            case BuiltInDataTypeKind.Texture3D:
                sb.Append("tex3d");
                break;
            case BuiltInDataTypeKind.Cubemap:
                sb.Append("texcube");
                break;
            case BuiltInDataTypeKind.ArrayCubemap:
                sb.Append("arraytexcube");
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (IsArray)
        {
            sb.Append('[');
            if (ArraySize != null)
                sb.Append(ArraySize);
            sb.Append(']');
        }

        return sb.ToString();
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

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion
}
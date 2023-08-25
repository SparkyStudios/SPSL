using System.Text;
using SPSL.Language.Core;

namespace SPSL.Language.AST;

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

    public override string ToString()
    {
        StringBuilder sb = new();

        switch (Type)
        {
            case PrimitiveDataTypeKind.Void:
                sb.Append("void");
                break;
            case PrimitiveDataTypeKind.Boolean:
                sb.Append("bool");
                break;
            case PrimitiveDataTypeKind.Integer:
                sb.Append("int");
                break;
            case PrimitiveDataTypeKind.UnsignedInteger:
                sb.Append("uint");
                break;
            case PrimitiveDataTypeKind.Float:
                sb.Append("float");
                break;
            case PrimitiveDataTypeKind.String:
                sb.Append("string");
                break;
            case PrimitiveDataTypeKind.Double:
                sb.Append("double");
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

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion

    #region IEquatable<IDataType> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(IDataType? other)
    {
        return other is PrimitiveDataType otherType && Type == otherType.Type && IsArray == otherType.IsArray &&
               ArraySize == otherType.ArraySize;
    }

    #endregion
}
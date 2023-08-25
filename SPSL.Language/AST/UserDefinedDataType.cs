namespace SPSL.Language.AST;

/// <summary>
/// Represents a data type created by the user in code.
/// </summary>
public class UserDefinedDataType : IDataType
{
    #region Properties

    /// <summary>
    /// The reference to the user-defined type.
    /// </summary>
    public NamespacedReference Type { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UserDefinedDataType"/>.
    /// </summary>
    /// <param name="type">The data type reference.</param>
    public UserDefinedDataType(NamespacedReference type)
    {
        type.Parent = this;

        Type = type;
    }

    #endregion

    #region Overrides

    public override string ToString() => Type.ToString();

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
        return other is UserDefinedDataType otherType && Type.Equals(otherType.Type) && IsArray == otherType.IsArray &&
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
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Type.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
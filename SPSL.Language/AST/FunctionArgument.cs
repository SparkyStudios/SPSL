using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL function argument.
/// </summary>
public class FunctionArgument : INode, IEquatable<FunctionArgument>
{
    #region Properties

    /// <summary>
    /// The flow of the data.
    /// </summary>
    public DataFlow Flow { get; }

    /// <summary>
    /// The data type.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The argument's name.
    /// </summary>
    public Identifier Name { get; }

    #endregion

    #region Constructors

    public FunctionArgument(DataFlow flow, IDataType type, Identifier name)
    {
        Flow = flow;
        Type = type;
        Name = name;
    }

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        return Equals(obj as FunctionArgument);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Flow, Type, Name, Start, End, Source);
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
        return Type.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region IEquatable<FunctionArgument> Implementation

    public bool Equals(FunctionArgument? other)
    {
        return other is not null && Flow == other.Flow && Type.Equals(other.Type) &&
               Name.Value.Equals(other.Name.Value);
    }

    #endregion
}
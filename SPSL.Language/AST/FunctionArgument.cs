using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL function argument.
/// </summary>
public class FunctionArgument : IDocumented, INode, ISemanticallyEquatable<FunctionArgument>,
    IEquatable<FunctionArgument>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionArgument"/> class.
    /// </summary>
    /// <param name="flow">The argument's data flow.</param>
    /// <param name="type">The argument's type.</param>
    /// <param name="name">The argument's name.</param>
    public FunctionArgument(DataFlow flow, IDataType type, Identifier name)
    {
        type.Parent = this;
        name.Parent = this;

        Flow = flow;
        Type = type;
        Name = name;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((FunctionArgument)obj);
    }

    /// <inheritdoc cref="object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine((int)Flow, Type, Name, Start, End, Source);
    }

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

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
        return Type.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable<FunctionArgument> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(FunctionArgument? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        // A function argument is semantically equivalent if it has the same flow constraint and the same type.
        return Flow == other.Flow && Type.SemanticallyEquals(other.Type);
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine((int)Flow, Type);
    }

    #endregion

    #region IEquatable<FunctionArgument> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(FunctionArgument? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Flow == other.Flow && Type.Equals(other.Type) &&
               Name.Equals(other.Name) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
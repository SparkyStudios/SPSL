namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a 64-bit floating-point value.
/// </summary>
public class DoubleLiteral : ILiteral, IEquatable<DoubleLiteral>
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public double Value => (double)((ILiteral)this).Value;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="DoubleLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public DoubleLiteral(double value)
    {
        ((ILiteral)this).Value = value;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((DoubleLiteral)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Start, End, Source);
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

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
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion

    #region IEquatable<BoolLiteral> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(DoubleLiteral? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Value.Equals(other.Value) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
namespace SPSL.Language.AST;

/// <summary>
/// Represents a unsigned integer value.
/// </summary>
public class UnsignedIntegerLiteral : ILiteral, IEquatable<UnsignedIntegerLiteral>
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public uint Value => (uint)((ILiteral)this).Value;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="UnsignedIntegerLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public UnsignedIntegerLiteral(uint value)
    {
        ((ILiteral)this).Value = value;
    }

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((UnsignedIntegerLiteral)obj);
    }

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
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion

    #region IEquatable<BoolLiteral> Implementation

    public bool Equals(UnsignedIntegerLiteral? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
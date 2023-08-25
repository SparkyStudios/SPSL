using System.Text;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an integer value.
/// </summary>
public class IntegerLiteral : ILiteral, IEquatable<IntegerLiteral>
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public int Value
    {
        get => (int)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    public bool IsOctalConstant { get; init; }

    public bool IsHexConstant { get; init; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="IntegerLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public IntegerLiteral(int value)
    {
        Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((IntegerLiteral)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Start, End, Source);
    }

    public override string ToString()
    {
        StringBuilder output = new();

        if (IsHexConstant)
            output.Append("0x");

        if (IsOctalConstant)
            output.Append('0');

        output.Append(Convert.ToString(Value, IsHexConstant ? 16 : IsOctalConstant ? 8 : 10));

        return output.ToString();
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

    #region IEquatable<BoolLiteral> Implementation

    public bool Equals(IntegerLiteral? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
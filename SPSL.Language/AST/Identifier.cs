namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL identifier.
/// </summary>
/// <example>
/// <code>aSimpleIdentifier</code>
/// </example>
public class Identifier : INode, IEquatable<Identifier>
{
    #region Properties

    /// <summary>
    /// The name of the identifier.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    #endregion

    #region Overrides

    public override string ToString()
    {
        return Value;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Identifier)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Value, Start, End, Source);
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
        return Source == source && offset >= Start && offset <= End ? this : null;
    }

    #endregion

    #region IEquatable<Identifier> Implementation

    public bool Equals(Identifier? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value && Start == other.Start && End == other.End && Source == other.Source;
    }

    #endregion
}
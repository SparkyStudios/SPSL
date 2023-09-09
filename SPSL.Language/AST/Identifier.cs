namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL identifier.
/// </summary>
/// <example>
/// <code>aSimpleIdentifier</code>
/// </example>
public class Identifier : INode, ISemanticallyEquatable<Identifier>, IEquatable<Identifier>
{
    #region Properties

    /// <summary>
    /// The name of the identifier.
    /// </summary>
    public string Value { get; init; } = string.Empty;

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)"/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Identifier)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()"/>
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

    #region ISemanticallyEquatable<Identifier> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(Identifier? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        // An identifier is semantically equivalent to another identifier if they have the same value.
        return Value == other.Value;
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Value);
    }

    #endregion

    #region IEquatable<Identifier> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(Identifier? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Value == other.Value && Start == other.Start && End == other.End && Source == other.Source;
    }

    #endregion
}
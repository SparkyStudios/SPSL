namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents the most basic expression in SPSL.
/// It generally reference an existing variable.
/// </summary>
public class BasicExpression : IAssignableExpression, ISemanticallyEquatable<BasicExpression>,
    IEquatable<BasicExpression>
{
    #region Properties

    /// <summary>
    /// The expression's identifier.
    /// </summary>
    public Identifier Identifier { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="BasicExpression"/>.
    /// </summary>
    /// <param name="identifier">The identifier of the expression.</param>
    public BasicExpression(Identifier identifier)
    {
        identifier.Parent = this;

        Identifier = identifier;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((BasicExpression)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()"/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, Start, End, Source);
    }

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
        return Identifier.ResolveNode(source, offset);
    }

    #endregion

    #region ISemanticallyEquatable<Annotation> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T)"/>
    public bool SemanticallyEquals(BasicExpression? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Identifier.SemanticallyEquals(other.Identifier);
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return Identifier.GetSemanticHashCode();
    }

    #endregion

    #region IEquatable<BasicExpression> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(BasicExpression? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Identifier.Equals(other.Identifier) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
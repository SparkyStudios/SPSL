namespace SPSL.Language.AST;

/// <summary>
/// Represents the most basic expression in SPSL.
/// It generally reference an existing variable.
/// </summary>
public class BasicExpression : IAssignableExpression
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
        return Identifier.ResolveNode(source, offset);
    }

    #endregion
}
namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an expression wrapped in parenthesis.
/// </summary>
public class ParenthesizedExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The wrapped expression.
    /// </summary>
    public IExpression Expression { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="ParenthesizedExpression"/>.
    /// </summary>
    /// <param name="expression">The expression wrapped in parenthesis.</param>
    public ParenthesizedExpression(IExpression expression)
    {
        expression.Parent = this;

        Expression = expression;
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
        return Expression.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
namespace SPSL.Language.AST;

/// <summary>
/// Represents a negate operation expression.
/// </summary>
public class NegateOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The expression to negate.
    /// </summary>
    public IExpression Expression { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="NegateOperationExpression"/>.
    /// </summary>
    /// <param name="expression">The expression to negate.</param>
    public NegateOperationExpression(IExpression expression)
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
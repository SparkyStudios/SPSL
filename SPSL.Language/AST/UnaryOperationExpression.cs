using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an unary operation expression. They are generally,
/// increment/decrement operations.
/// </summary>
public class UnaryOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The unary operator.
    /// </summary>
    public Op Operator { get; }

    /// <summary>
    /// The expression on which operate.
    /// </summary>
    public IAssignableExpression Expression { get; }

    /// <summary>
    /// The type of the expression.
    /// </summary>
    public bool IsPostfix { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UnaryOperationExpression"/>.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="op">The operator.</param>
    /// <param name="isPostfix">Whether the operator is place before or after the expression.</param>
    public UnaryOperationExpression(IAssignableExpression expression, Op op, bool isPostfix = false)
    {
        expression.Parent = this;

        Expression = expression;
        Operator = op;
        IsPostfix = isPostfix;
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
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
    public string Operator { get; set; }

    /// <summary>
    /// The expression on which operate.
    /// </summary>
    public IAssignableExpression Expression { get; set; }

    /// <summary>
    /// The type of the expression.
    /// </summary>
    public bool IsPostfix { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UnaryOperationExpression"/>.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="op"></param>
    /// <param name="isPostfix"></param>
    public UnaryOperationExpression(IAssignableExpression expression, string op, bool isPostfix = false)
    {
        Expression = expression;
        Operator = op;
        IsPostfix = isPostfix;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
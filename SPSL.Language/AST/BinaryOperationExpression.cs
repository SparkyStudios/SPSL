namespace SPSL.Language.AST;

/// <summary>
/// Represent a binary operation expression.
/// </summary>
public class BinaryOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The left operand expression.
    /// </summary>
    public IExpression Left { get; set; }

    /// <summary>
    /// The operator.
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// The right operand expression.
    /// </summary>
    public IExpression Right { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="BinaryOperationExpression"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="op">The operator.</param>
    /// <param name="right">The right operand.</param>
    public BinaryOperationExpression(IExpression left, string op, IExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
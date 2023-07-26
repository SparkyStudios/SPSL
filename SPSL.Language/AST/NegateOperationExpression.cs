namespace SPSL.Language.AST;

/// <summary>
/// Represent a negate operation expression.
/// </summary>
public class NegateOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The expression to negate.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="NegateOperationExpression"/>.
    /// </summary>
    /// <param name="expression">The expression to negate.</param>
    public NegateOperationExpression(IExpression expression)
    {
        Expression = expression;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
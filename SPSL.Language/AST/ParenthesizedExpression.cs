namespace SPSL.Language.AST;

/// <summary>
/// Represent an expression wrapped in parenthesis.
/// </summary>
public class ParenthesizedExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The wrapped expression.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="ParenthesizedExpression"/>.
    /// </summary>
    /// <param name="expression">The expression wrapped in parenthesis.</param>
    public ParenthesizedExpression(IExpression expression)
    {
        Expression = expression;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
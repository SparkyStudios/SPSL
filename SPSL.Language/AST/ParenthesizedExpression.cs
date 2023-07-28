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

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
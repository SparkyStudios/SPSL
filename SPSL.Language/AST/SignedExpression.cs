namespace SPSL.Language.AST;

/// <summary>
/// Represents an expression prefixed with a + or - sign
/// </summary>
public class SignedExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The operation sign.
    /// </summary>
    /// <value>
    /// + or -.
    /// </value>
    public string Sign { get; set; }

    /// <summary>
    /// The expression.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SignedExpression"/> class.
    /// </summary>
    public SignedExpression(string sign, IExpression expression)
    {
        Sign = sign;
        Expression = expression;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
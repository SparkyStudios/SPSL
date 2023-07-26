namespace SPSL.Language.AST;

/// <summary>
/// Represent an expression, used as a statement.
/// </summary>
public class ExpressionStatement : IStatement
{
    #region Properties

    /// <summary>
    /// The wrapped expression.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="ExpressionStatement"/>.
    /// </summary>
    /// <param name="expression">The wrapped expression.</param>
    public ExpressionStatement(IExpression expression)
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
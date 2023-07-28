namespace SPSL.Language.AST;

/// <summary>
/// Represent a <c>return</c> statement.
/// </summary>
public class ReturnStatement : IStatement
{
    #region Properties

    /// <summary>
    /// The expression to return.
    /// </summary>
    public IExpression? Expression { get; } = null;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="ReturnStatement"/>.
    /// </summary>
    /// <param name="expression">The expression to return.</param>
    public ReturnStatement(IExpression? expression)
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
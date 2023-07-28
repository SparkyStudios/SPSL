namespace SPSL.Language.AST;

/// <summary>
/// Represent an invocation parameter.
/// </summary>
public class InvocationParameter : INode
{
    #region Properties

    /// <summary>
    /// The invocation parameter expression.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="InvocationParameter"/>.
    /// </summary>
    /// <param name="expression">The parameter expression.</param>
    public InvocationParameter(IExpression expression)
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
namespace SPSL.Language.AST;

/// <summary>
/// Represent a ternary operation.
/// </summary>
public class TernaryOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The ternary operation condition.
    /// </summary>
    public IExpression Condition { get; set; }

    /// <summary>
    /// The expression to return when <see cref="Condition"/>
    /// is evaluated to true.
    /// </summary>
    public IExpression WhenTrue { get; set; }

    /// <summary>
    /// The expression to return when <see cref="Condition"/>
    /// is evaluated to false.
    /// </summary>
    public IExpression WhenFalse { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="TernaryOperationExpression"/>.
    /// </summary>
    /// <param name="condition">The condition expression.</param>
    /// <param name="whenTrue">The expression to return when <paramref name="condition"/> is evaluated to <c>true</c>.</param>
    /// <param name="whenFalse">The expression to return when <paramref name="condition"/> is evaluated to <c>false</c>.</param>
    public TernaryOperationExpression(IExpression condition, IExpression whenTrue, IExpression whenFalse)
    {
        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
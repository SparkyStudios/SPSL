namespace SPSL.Language.AST;

/// <summary>
/// Represent an array access expression.
/// </summary>
public class ArrayAccessExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The expression which returns an array.
    /// </summary>
    public IExpression Expression { get; set; }

    /// <summary>
    /// The expression returning the array index.
    /// </summary>
    public IExpression ArrayIndex { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="ArrayAccessExpression"/>
    /// </summary>
    /// <param name="expression">The expression returning the array.</param>
    /// <param name="arrayIndex">The expression returning the array index.</param>
    public ArrayAccessExpression(IExpression expression, IExpression arrayIndex)
    {
        Expression = expression;
        ArrayIndex = arrayIndex;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
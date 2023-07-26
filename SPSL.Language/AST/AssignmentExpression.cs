namespace SPSL.Language.AST;

/// <summary>
/// Represent a value assignment.
/// </summary>
public class AssignmentExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The left operand, the expression in which define the value.
    /// </summary>
    public IAssignableExpression Left { get; set; }

    /// <summary>
    /// The assignment operator.
    /// </summary>
    public string Operator { get; set; }

    /// <summary>
    /// The right operand, the value to assign.
    /// </summary>
    public IExpression Right { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="AssignmentExpression"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="op">The assignment operator.</param>
    /// <param name="right">The right operand.</param>
    public AssignmentExpression(IAssignableExpression left, string op, IExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
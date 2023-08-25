namespace SPSL.Language.AST;

/// <summary>
/// Represents a SPSL value assignment.
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
        left.Parent = this;
        right.Parent = this;

        Left = left;
        Operator = op;
        Right = right;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Left.ResolveNode(source, offset) ?? Right.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
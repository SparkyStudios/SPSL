using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a binary operation expression in SPSL.
/// </summary>
public class BinaryOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The left operand expression.
    /// </summary>
    public IExpression Left { get; }

    /// <summary>
    /// The operator.
    /// </summary>
    public Op Operator { get; }

    /// <summary>
    /// The right operand expression.
    /// </summary>
    public IExpression Right { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="BinaryOperationExpression"/>.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="op">The operator.</param>
    /// <param name="right">The right operand.</param>
    public BinaryOperationExpression(IExpression left, Op op, IExpression right)
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
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Left.ResolveNode(source, offset) ?? Right.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
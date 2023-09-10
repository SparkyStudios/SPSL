namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a ternary operation.
/// </summary>
public class TernaryOperationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The ternary operation condition.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// The expression to return when <see cref="Condition"/>
    /// is evaluated to true.
    /// </summary>
    public IExpression WhenTrue { get; }

    /// <summary>
    /// The expression to return when <see cref="Condition"/>
    /// is evaluated to false.
    /// </summary>
    public IExpression WhenFalse { get; }

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
        condition.Parent = this;
        whenTrue.Parent = this;
        whenFalse.Parent = this;

        Condition = condition;
        WhenTrue = whenTrue;
        WhenFalse = whenFalse;
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
        return Condition.ResolveNode(source, offset) ?? WhenTrue.ResolveNode(source, offset) ??
            WhenFalse.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
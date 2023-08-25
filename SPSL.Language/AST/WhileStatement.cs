namespace SPSL.Language.AST;

public class WhileStatement : IStatement
{
    #region Properties

    /// <summary>
    /// The while statement's condition.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// The set of statements to execute while the <see cref="Condition"/>
    /// is evaluated to true.
    /// </summary>
    public StatementBlock Block { get; }

    #endregion

    #region Constructors

    public WhileStatement(IExpression condition, StatementBlock block)
    {
        condition.Parent = this;
        block.Parent = this;

        Condition = condition;
        Block = block;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Condition.ResolveNode(source, offset) ?? Block.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
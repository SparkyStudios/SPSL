using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class StatementBlock : IStatement
{
    #region Constructors

    internal StatementBlock(OrderedSet<IStatement> children)
    {
        Children = children;
    }

    public StatementBlock(params IStatement[] children)
    {
        Children = new(children);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IStatement> Children { get; }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
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
        Children = new OrderedSet<IStatement>(children);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IStatement> Children { get; }

    #endregion

}
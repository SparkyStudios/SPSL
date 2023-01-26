using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class StatementBlock : IStatement, IBlock
{
    #region Constructors

    internal StatementBlock(OrderedSet<IStatement> children)
    {
        Children = new OrderedSet<IBlockChild>(children);
    }

    public StatementBlock(params IStatement[] children)
    {
        Children = new OrderedSet<IBlockChild>(children);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; }

    #endregion

}
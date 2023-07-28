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

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
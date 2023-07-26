namespace SPSL.Language.AST;

public class Function : IBlockChild
{
    #region Properties

    public FunctionHead Head { get; set; }

    public StatementBlock Body { get; set; }

    #endregion

    #region Constructors

    public Function(FunctionHead head, StatementBlock body)
    {
        Head = head;
        Body = body;
    }

    public Function(FunctionHead head, params IStatement[] children)
    {
        Head = head;
        Body = new(children)
        {
            Start = children[0].Start,
            End = children[^1].End
        };
    }

    #endregion

    #region IBlockChild Implementation

    string IBlockChild.Name
    {
        get => Head.Name;
        set => Head.Name = value;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
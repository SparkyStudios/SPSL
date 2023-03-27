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
        Body = new StatementBlock(children);
    }

    #endregion

    #region IBlockChild Implementation

    string IBlockChild.Name { get => Head.Name; set => Head.Name = value; }

    #endregion
}
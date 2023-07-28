namespace SPSL.Language.AST;

public class InvalidStatement : IStatement
{
    #region INode Implementation

    public string Source { get; set; } = null!;
    
    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
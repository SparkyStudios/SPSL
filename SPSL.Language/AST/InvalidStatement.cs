namespace SPSL.Language.AST;

public class InvalidStatement : IStatement
{
    #region INode Implementation

    public string Source { get; set; }
    
    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
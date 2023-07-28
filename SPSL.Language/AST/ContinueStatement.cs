namespace SPSL.Language.AST;

/// <summary>
/// Represent a <c>continue</c> statement.
/// </summary>
public class ContinueStatement : IStatement
{
    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
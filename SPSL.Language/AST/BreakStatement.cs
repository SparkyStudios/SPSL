namespace SPSL.Language.AST;

/// <summary>
/// Represent a <c>break</c> statement.
/// </summary>
public class BreakStatement : IStatement
{
    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
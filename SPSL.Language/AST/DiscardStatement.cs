namespace SPSL.Language.AST;

/// <summary>
/// Represent a <c>skip</c> statement.
/// </summary>
public class DiscardStatement : IStatement
{
    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
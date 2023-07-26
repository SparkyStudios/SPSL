namespace SPSL.Language.AST;

/// <summary>
/// Represent a <c>skip</c> statement.
/// </summary>
public class DiscardStatement : IStatement
{
    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
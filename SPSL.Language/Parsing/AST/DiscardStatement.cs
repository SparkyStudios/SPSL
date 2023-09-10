namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a <c>discard</c> statement.
/// </summary>
public class DiscardStatement : ILeaveControlFlowStatement
{
    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Source == source && offset >= Start && offset <= End ? this as INode : null;
    }

    #endregion
}
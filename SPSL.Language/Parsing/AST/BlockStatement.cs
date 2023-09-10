using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a set of statements in SPSL.
/// </summary>
public class StatementBlock : IStatement
{
    #region Constructors

    internal StatementBlock(OrderedSet<IStatement> children)
    {
        foreach (IStatement child in children)
            child.Parent = this;

        Children = children;
    }

    public StatementBlock(params IStatement[] children)
    {
        foreach (IStatement child in children)
            child.Parent = this;

        Children = new(children);
    }

    #endregion

    #region IBlock Implementation

    /// <inheritdoc cref="IBlock.Children" />
    public OrderedSet<IStatement> Children { get; }

    #endregion

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
        return Children.FirstOrDefault(child => child.ResolveNode(source, offset) != null)
                   ?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
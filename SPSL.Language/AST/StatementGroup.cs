using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a collection of statements.
/// </summary>
public class StatementCollection : IStatement
{
    #region Properties

    /// <summary>
    /// The collection of statements.
    /// </summary>
    public OrderedSet<IStatement> Statements { get; }

    #endregion

    #region Constructors

    public StatementCollection(IEnumerable<IStatement> statements)
    {
        Statements = new(statements);

        foreach (IStatement statement in Statements)
            statement.Parent = this;
    }

    public StatementCollection(params IStatement[] statements)
    {
        Statements = new(statements);

        foreach (IStatement statement in Statements)
            statement.Parent = this;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Statements.FirstOrDefault(s => s.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
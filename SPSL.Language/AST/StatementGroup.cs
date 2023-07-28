using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent a collection of statements.
/// </summary>
public class StatementCollection : IStatement
{
    #region Properties

    /// <summary>
    /// The collection of statements.
    /// </summary>
    public OrderedSet<IStatement> Statements { get; set; }

    #endregion

    #region Constructors

    public StatementCollection(IEnumerable<IStatement> statements)
    {
        Statements = new OrderedSet<IStatement>(statements);
    }

    public StatementCollection(params IStatement[] statements)
    {
        Statements = new OrderedSet<IStatement>(statements);
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
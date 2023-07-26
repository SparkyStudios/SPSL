using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an if condition statement.
/// </summary>
public class IfStatement : IStatement
{
    #region Nested Types

    public class IfStatementConditionBlock
    {
        #region Properties

        /// <summary>
        /// The if statement's condition.
        /// </summary>
        public IExpression Condition { get; init; } = null!;

        /// <summary>
        /// The set of statement to execute if the <see cref="Condition"/>
        /// is evaluated to <c>true</c>
        /// </summary>
        public StatementBlock Block { get; init; } = null!;

        #endregion
    }

    #endregion

    #region Properties

    /// <summary>
    /// The if part of the statement.
    /// </summary>
    public IfStatementConditionBlock If { get; set; }

    /// <summary>
    /// The collection of elif parts of the statement. Can be null.
    /// </summary>
    public OrderedSet<IfStatementConditionBlock> Elif { get; set; }

    /// <summary>
    /// The else part of the statement. Can be null.
    /// </summary>
    public StatementBlock? Else { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="IfStatement"/>.
    /// </summary>
    /// <param name="if">The if part of the statement.</param>
    /// <param name="elif">The elif parts of the statement.</param>
    /// <param name="else">The else part of the statement.</param>
    public IfStatement
    (
        IfStatementConditionBlock @if,
        IEnumerable<IfStatementConditionBlock>? elif = null,
        StatementBlock? @else = null
    )
    {
        If = @if;
        Elif = new(elif);
        Else = @else;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
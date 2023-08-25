using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an if condition statement.
/// </summary>
public class IfStatement : IStatement
{
    #region Nested Types

    public class IfStatementConditionBlock : INode
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
            return Condition.ResolveNode(source, offset) ?? Block.ResolveNode(source, offset) ??
                (Source == source && offset >= Start && offset <= End ? this as INode : null);
        }

        #endregion
    }

    #endregion

    #region Properties

    /// <summary>
    /// The if part of the statement.
    /// </summary>
    public IfStatementConditionBlock If { get; }

    /// <summary>
    /// The collection of elif parts of the statement. Can be empty.
    /// </summary>
    public OrderedSet<IfStatementConditionBlock> Elif { get; }

    /// <summary>
    /// The else part of the statement. Can be null.
    /// </summary>
    public StatementBlock? Else { get; }

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
        @if.Parent = this;

        if (@else is not null)
            @else.Parent = this;

        If = @if;
        Elif = new(elif);
        Else = @else;

        if (elif is not null)
            foreach (IfStatementConditionBlock e in Elif)
                e.Parent = this;
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
        return If.ResolveNode(source, offset) ?? Elif.FirstOrDefault(b => b.ResolveNode(source, offset) is not null)
                ?.ResolveNode(source, offset) ??
            Else?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
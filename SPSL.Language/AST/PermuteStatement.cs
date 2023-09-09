using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Allows to use permutation variables on a block of code.
/// </summary>
public class PermuteStatement : IStatement
{
    #region Properties

    /// <summary>
    /// The if statement's condition.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// The set of statement to execute if the <see cref="Condition"/>
    /// is evaluated to <c>true</c>
    /// </summary>
    public StatementBlock Block { get; }

    /// <summary>
    /// The else part of the statement. Can be null.
    /// </summary>
    public StatementBlock? Else { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PermuteStatement"/>.
    /// </summary>
    /// <param name="condition">The condition on which activate the statements <paramref name="block"/>.</param>
    /// <param name="block">The statements to add in the code when the <paramref name="condition"/> is evaluated to true.</param>
    /// <param name="else">An optional block of code to inject when the <paramref name="condition"/> is evaluated to false.</param>
    public PermuteStatement(IExpression condition, StatementBlock block, StatementBlock? @else = null)
    {
        condition.Parent = this;
        block.Parent = this;
        if (@else != null)
            @else.Parent = this;

        Condition = condition;
        Block = block;
        Else = @else;
    }

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
        return Condition.ResolveNode(source, offset) ?? Block.ResolveNode(source, offset) ??
            Else?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
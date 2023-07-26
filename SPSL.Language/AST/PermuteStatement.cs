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
    public IExpression Condition { get; init; }

    /// <summary>
    /// The set of statement to execute if the <see cref="Condition"/>
    /// is evaluated to <c>true</c>
    /// </summary>
    public StatementBlock Block { get; init; }

    /// <summary>
    /// The else part of the statement. Can be null.
    /// </summary>
    public StatementBlock? Else { get; set; }

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
        Condition = condition;
        Block = block;
        Else = @else;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
namespace SPSL.Language.AST;

public class WhileStatement : IStatement
{
    #region Properties

    /// <summary>
    /// The while statement's condition.
    /// </summary>
    public IExpression Condition { get; init; } = null!;

    /// <summary>
    /// The set of statements to execute while the <see cref="Condition"/>
    /// is evaluated to true.
    /// </summary>
    public StatementBlock Block { get; init; } = null!;

    #endregion
}
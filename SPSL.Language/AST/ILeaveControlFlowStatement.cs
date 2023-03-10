namespace SPSL.Language.AST;

/// <summary>
/// Represent a statement which leave the current
/// control flow, and move back to the parent one, if any.
/// </summary>
public interface ILeaveControlFlowStatement : IStatement
{ }
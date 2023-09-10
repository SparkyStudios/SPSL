namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a statement which leave the current
/// control flow, and move back to the parent one, if any.
/// </summary>
public interface ILeaveControlFlowStatement : IStatement
{ }
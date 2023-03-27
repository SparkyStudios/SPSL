namespace SPSL.Language.AST;

/// <summary>
/// Represent a variable declaration statement.
/// </summary>
public class VariableDeclarationStatement : IStayControlFlowStatement
{
    #region Properties

    /// <summary>
    /// Defines if the variable is declared using the "const" modifier.
    /// </summary>
    public bool IsConst { get; init; } = false;
    
    /// <summary>
    /// The type of the variable.
    /// </summary>
    public IDataType Type { get; init; } = null!;

    /// <summary>
    /// The name of the variable.
    /// </summary>
    public BasicExpression Name { get; init; } = null!;

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression? Initializer { get; init; } = null;

    #endregion
}
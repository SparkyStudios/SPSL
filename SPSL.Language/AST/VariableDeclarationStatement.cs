namespace SPSL.Language.AST;

/// <summary>
/// Represent a variable declaration statement.
/// </summary>
public class VariableDeclarationStatement : IStayControlFlowStatement
{
    #region Properties

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public IDataType Type { get; set; }

    /// <summary>
    /// The name of the variable.
    /// </summary>
    public BasicExpression Name { get; set; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression? Initializer { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="VariableDeclarationStatement"/>.
    /// </summary>
    /// <param name="type">The type of the variable.</param>
    /// <param name="name">The name of the variable.</param>
    /// <param name="initialValue">The initial value of the variable.</param>
    public VariableDeclarationStatement(IDataType type, BasicExpression name, IExpression? initialValue = null)
    {
        Type = type;
        Name = name;
        Initializer = initialValue;
    }

    #endregion
}
namespace SPSL.Language.AST;

/// <summary>
/// An SPSL shader global variable.
/// </summary>
public class GlobalVariable : IShaderMember
{
    #region Properties

    /// <summary>
    /// Whether the variable is declared with the "static" modifier.
    /// </summary>
    /// <value>
    /// <c>true</c> if the variable is declared with the "static" modifier; otherwise
    /// <c>false</c>.
    /// </value>
    public bool IsStatic { get; set; }

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public IDataType Type { get; set; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression Initializer { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalVariable"/> class.
    /// </summary>
    public GlobalVariable(bool isStatic, IDataType type, IExpression initializer)
    {
        IsStatic = isStatic;
        Type = type;
        Initializer = initializer;
    }

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The type name.
    /// </summary>
    public string Name { get; set; } = null!;

    #endregion
    
    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
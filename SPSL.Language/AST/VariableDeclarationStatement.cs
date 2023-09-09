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
    public bool IsConst { get; init; }

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The name of the variable.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression? Initializer { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDeclarationStatement"/> class.
    /// </summary>
    /// <param name="type">The variable's type.</param>
    /// <param name="name">The variable's name.</param>
    /// <param name="initializer">The initial value of the variable.</param>
    public VariableDeclarationStatement(IDataType type, Identifier name, IExpression? initializer = null)
    {
        type.Parent = this;
        name.Parent = this;

        if (initializer != null)
            initializer.Parent = this;

        Type = type;
        Name = name;
        Initializer = initializer;
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
        return Type.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Initializer?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
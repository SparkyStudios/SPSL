namespace SPSL.Language.Parsing.AST;

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
    /// <c>true</c> if the variable is declared with the "static"
    /// modifier; otherwise <c>false</c>.
    /// </value>
    public bool IsStatic { get; }

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression Initializer { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalVariable"/> class.
    /// </summary>
    public GlobalVariable(bool isStatic, IDataType type, IExpression initializer)
    {
        type.Parent = this;
        initializer.Parent = this;

        IsStatic = isStatic;
        Type = type;
        Initializer = initializer;
    }

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The type name.
    /// </summary>
    public Identifier Name { get; set; } = null!;

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

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
        return Type.ResolveNode(source, offset) ?? Initializer.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
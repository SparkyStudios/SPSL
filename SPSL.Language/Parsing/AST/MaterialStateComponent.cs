namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a material state component.
/// </summary>
public class MaterialStateComponent : IBlockChild
{
    #region Properties

    /// <summary>
    /// The value of the <see cref="MaterialStateComponent"/>.
    /// </summary>
    public IExpression Value { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialStateComponent"/> class.
    /// </summary>
    /// <param name="name">The component's name.</param>
    /// <param name="value">The component's value.</param>
    public MaterialStateComponent(Identifier name, IExpression value)
    {
        name.Parent = this;
        value.Parent = this;

        Name = name;
        Value = value;
    }

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name"/>
    public Identifier Name { get; set; }

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
        return Value.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
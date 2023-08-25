namespace SPSL.Language.AST;

public class MaterialStateComponent : IBlockChild
{
    #region Properties

    /// <summary>
    /// The value of the <see cref="MaterialStateComponent"/>.
    /// </summary>
    public IExpression Value { get; }

    #endregion

    #region Constructors

    public MaterialStateComponent(Identifier name, IExpression value)
    {
        name.Parent = this;
        value.Parent = this;
        
        Name = name;
        Value = value;
    }

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The component name.
    /// </summary>
    public Identifier Name { get; set; }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Value.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
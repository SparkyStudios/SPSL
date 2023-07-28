namespace SPSL.Language.AST;

public class MaterialStateComponent : IBlockChild
{
    #region Properties

    /// <summary>
    /// The value of the <see cref="MaterialStateComponent"/>.
    /// </summary>
    public IExpression Value { get; set; }

    #endregion

    #region Constructors

    public MaterialStateComponent(string name, IExpression value)
    {
        Name = name;
        Value = value;
    }

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The component name.
    /// </summary>
    public string Name { get; set; }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
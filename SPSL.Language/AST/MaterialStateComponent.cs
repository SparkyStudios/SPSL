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
}

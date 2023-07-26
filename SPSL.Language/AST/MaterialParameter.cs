using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// A single parameter within a <see cref="MaterialParameterGroup"/> of
/// an SPSL <see cref="Material"/>.
/// </summary>
public class MaterialParameter : IAnnotable, IBlockChild
{
    #region Properties

    /// <summary>
    /// The parameter type.
    /// </summary>
    /// <value>
    /// Whether this parameter affects a permutation variable or a constant buffer variable.
    /// </value>
    public MaterialParameterType Type { get; set; }

    /// <summary>
    /// The parameter default value.
    /// </summary>
    public IExpression? DefaultValue { get; set; }

    /// <summary>
    /// The type of the value of this parameter.
    /// </summary>
    public IDataType ValueType { get; set; }

    #endregion

    #region Constructors

    public MaterialParameter(IDataType valueType, string name, MaterialParameterType type)
    {
        ValueType = valueType;
        Name = name;
        Type = type;
        DefaultValue = null;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The parameter name.
    /// </summary>
    public string Name { get; set; }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
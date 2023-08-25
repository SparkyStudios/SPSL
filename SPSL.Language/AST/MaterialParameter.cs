using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// A single parameter within a <see cref="MaterialParameterGroup"/> of
/// an SPSL <see cref="Material"/>.
/// </summary>
public class MaterialParameter : IAnnotated, IBlockChild
{
    #region Properties

    /// <summary>
    /// The parameter type.
    /// </summary>
    /// <value>
    /// Whether this parameter affects a permutation variable or a constant buffer variable.
    /// </value>
    public MaterialParameterType Type { get; }

    /// <summary>
    /// The parameter default value.
    /// </summary>
    public IExpression? DefaultValue { get; init; }

    /// <summary>
    /// The type of the value of this parameter.
    /// </summary>
    public IDataType ValueType { get; }

    #endregion

    #region Constructors

    public MaterialParameter(IDataType valueType, Identifier name, MaterialParameterType type)
    {
        valueType.Parent = this;
        name.Parent = this;
        
        ValueType = valueType;
        Name = name;
        Type = type;
        DefaultValue = null;
    }

    #endregion

    #region IAnnotated Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The parameter name.
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
        return DefaultValue?.ResolveNode(source, offset) ?? ValueType.ResolveNode(source, offset) ??
            Name.ResolveNode(source, offset) ??
            Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
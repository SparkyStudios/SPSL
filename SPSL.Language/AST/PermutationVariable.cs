using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// An SPSL shader permutation variable.
/// </summary>
public class PermutationVariable : INamespaceChild, IBlockChild
{
    #region Properties

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public PermutationVariableType Type { get; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression Initializer { get; }

    /// <summary>
    /// An array of enumeration values in case of an enumerated permutation value.
    /// </summary>
    public Identifier[] EnumerationValues { get; init; } = Array.Empty<Identifier>();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PermutationVariable"/> class.
    /// </summary>
    public PermutationVariable(PermutationVariableType type, Identifier name, IExpression initializer)
    {
        name.Parent = this;
        initializer.Parent = this;

        Type = type;
        Name = name;
        Initializer = initializer;
    }

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? ParentNamespace { get; set; }

    /// <summary>
    /// The permutation variable name.
    /// </summary>
    public Identifier Name { get; set; }

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation" />
    public string Documentation { get; init; } = string.Empty;

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Name.ResolveNode(source, offset) ?? Initializer.ResolveNode(source, offset) ??
            EnumerationValues.FirstOrDefault(v => v.ResolveNode(source, offset) != null)
                ?.ResolveNode(source, offset);
    }

    #endregion
}
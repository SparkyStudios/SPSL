using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// An SPSL shader permutation variable.
/// </summary>
public class PermutationVariable : INamespaceChild, IAnnotated, IBlockChild, ISemanticallyEquatable
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

    /// <inheritdoc cref="INamespaceChild.ParentNamespace"/>
    public Namespace? ParentNamespace { get; set; }

    /// <inheritdoc cref="INamespaceChild.Name"/>
    public Identifier Name { get; set; }

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations"/>
    public OrderedSet<Annotation> Annotations { get; init; } = new();

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
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Name.ResolveNode(source, offset) ?? Initializer.ResolveNode(source, offset) ??
            EnumerationValues.FirstOrDefault(v => v.ResolveNode(source, offset) != null)
                ?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not PermutationVariable other) return false;

        // Two permutation variables are semantically equal if they have the same type and same name.
        return Type == other.Type && Name.SemanticallyEquals(other.Name);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Type, Name.GetSemanticHashCode());
    }

    #endregion
}
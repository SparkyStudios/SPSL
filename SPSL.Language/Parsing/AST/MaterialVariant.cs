using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an array of permutations values to applies to a material at the same time.
/// Each group of permutation generates a different material binary.
/// </summary>
public class MaterialVariant : IMaterialMember
{
    #region Properties

    /// <summary>
    /// The list of permutation values in this group.
    /// </summary>
    public OrderedSet<IExpression> PermutationValues { get; init; } = new();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialVariant"/> class.
    /// </summary>
    /// <param name="name">The name of the permutation group.</param>
    public MaterialVariant(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name" />
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
        return PermutationValues.FirstOrDefault(x => x.ResolveNode(source, offset) != null)
                   ?.ResolveNode(source, offset) ??
               Name.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Setup a graphics state from the <see cref="Material"/>.
/// </summary>
public class MaterialState : IBlock, IMaterialMember
{
    #region Properties

    /// <summary>
    /// The type of the <see cref="MaterialState"/>.
    /// </summary>
    /// <value>
    /// If the <see cref="Type"/> is <see cref="MaterialStateType.Block"/>, the <see cref="Children"/>
    /// property will hold the <see cref="MaterialStateComponent"/> of this <see cref="MaterialState"/>.
    /// Otherwise, the <see cref="Children"/> property will be empty, and the <see cref="Value"/> property will
    /// have the value assigned to the <see cref="MaterialState"/>.
    /// </value>
    public MaterialStateType Type { get; init; }

    /// <summary>
    /// The value of the <see cref="MaterialState"/>.
    /// </summary>
    /// <value>
    /// The value of the <see cref="MaterialState"/> if the <see cref="Type"/> is set to <see cref="MaterialStateType.Value"/>.
    /// Otherwise <c>null</c>.
    /// </value>
    public string? Value { get; init; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialState"/> class with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the material state.</param>
    public MaterialState(Identifier name)
    {
        name.Parent = this;

        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialState"/> class.
    /// </summary>
    /// <param name="name">The name of the material state.</param>
    /// <param name="components">The list of components in the state.</param>
    public MaterialState(Identifier name, IEnumerable<MaterialStateComponent> components)
        : this(name)
    {
        Children.AddRange(components);

        foreach (IBlockChild child in Children)
            child.Parent = this;
    }

    #endregion

    #region IBlock Implementation

    /// <inheritdoc cref="IBlock.Children"/>
    public OrderedSet<IBlockChild> Children { get; } = new();

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
        return Name.ResolveNode(source, offset) ??
               Children.FirstOrDefault(c => c.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
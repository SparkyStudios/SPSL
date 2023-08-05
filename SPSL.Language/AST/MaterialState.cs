using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

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
    public MaterialStateType Type { get; set; }

    /// <summary>
    /// The value of the <see cref="MaterialState"/>.
    /// </summary>
    /// <value>
    /// The value of the <see cref="MaterialState"/> if the <see cref="Type"/> is set to <see cref="MaterialStateType.Value"/>.
    /// Otherwise <c>null</c>.
    /// </value>
    public string? Value { get; set; } = null;

    #endregion

    #region Constructors

    public MaterialState(string name)
    {
        Name = name;
    }

    public MaterialState(string name, IEnumerable<MaterialStateComponent> components)
    {
        Name = name;
        Children.AddRange(components);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

    #endregion

    #region IBlockChild Implementation

    /// <summary>
    /// The material state name.
    /// </summary>
    public string Name { get; set; }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
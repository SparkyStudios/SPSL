using SPSL.Language.AST;

namespace SPSL.Language.Core;

/// <summary>
/// Defines the type of a <see cref="MaterialState"/>.
/// </summary>
public enum MaterialStateType
{
    /// <summary>
    /// A <see cref="MaterialState"/> that represents a block of <see cref="MaterialStateComponent"/>s.
    /// </summary>
    Block,

    /// <summary>
    /// A <see cref="MaterialState"/> that represents a single value.
    /// </summary>
    Value,
}

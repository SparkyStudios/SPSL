namespace SPSL.Language.Core;

/// <summary>
/// The type of an SPSL permutation variable.
/// </summary>
public enum PermutationVariableType : byte
{
    /// <summary>
    /// The permutation var stores boolean values.
    /// </summary>
    Bool,

    /// <summary>
    /// The permutation var stores a defined set of values.
    /// Those values are defined at the same time than the variable.
    /// </summary>
    Enum,

    /// <summary>
    /// The permutation var stores integer values.
    /// </summary>
    Integer
}

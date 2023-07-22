namespace SPSL.Language.AST;

/// <summary>
/// Specifies the type of a parameter in a material.
/// </summary>
public enum MaterialParameterType
{
    /// <summary>
    /// The parameter affect permutation variables.
    /// </summary>
    Permutation,

    /// <summary>
    /// The parameter affects the value of a variable in a constant buffer.
    /// </summary>
    Value,
}

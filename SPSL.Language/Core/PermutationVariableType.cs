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

public static class PermutationVariableTypeExtensions
{
    public static bool IsBool(this PermutationVariableType type) => type == PermutationVariableType.Bool;

    public static bool IsEnum(this PermutationVariableType type) => type == PermutationVariableType.Enum;

    public static bool IsInteger(this PermutationVariableType type) => type == PermutationVariableType.Integer;

    public static string GetTypeName(this PermutationVariableType type) => type switch
    {
        PermutationVariableType.Bool => "bool",
        PermutationVariableType.Enum => "enum",
        PermutationVariableType.Integer => "int",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
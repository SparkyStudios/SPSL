using SPSL.Language.Core;

namespace SPSL.Language.Symbols.Modifiers;

public class PermutationTypeModifier : ISymbolModifier
{
    #region Properties

    public PermutationVariableType Type { get; }

    public HashSet<string> EnumValues { get; init; }

    #endregion

    #region Constructors

    public PermutationTypeModifier(PermutationVariableType type, params string[] enumValues)
    {
        Type = type;
        EnumValues = new(enumValues);
    }

    #endregion
}

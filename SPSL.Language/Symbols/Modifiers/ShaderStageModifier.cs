using SPSL.Language.Core;

namespace SPSL.Language.Symbols.Modifiers;

public class ShaderStageModifier : ISymbolModifier
{
    public ShaderStage Stage { get; }

    public ShaderStageModifier(ShaderStage stage)
    {
        Stage = stage;
    }
}
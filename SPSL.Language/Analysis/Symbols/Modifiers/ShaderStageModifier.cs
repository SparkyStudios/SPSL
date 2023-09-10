using SPSL.Language.Parsing.Common;

namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class ShaderStageModifier : ISymbolModifier
{
    public ShaderStage Stage { get; }

    public ShaderStageModifier(ShaderStage stage)
    {
        Stage = stage;
    }
}
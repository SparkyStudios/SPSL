using SPSL.Language.Parsing.Common;

namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class BufferAccessModifier : ISymbolModifier
{
    public BufferAccess Access { get; }

    public BufferAccessModifier(BufferAccess access)
    {
        Access = access;
    }
}
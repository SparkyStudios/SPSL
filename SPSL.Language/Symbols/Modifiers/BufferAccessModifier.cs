using SPSL.Language.Core;

namespace SPSL.Language.Symbols.Modifiers;

public class BufferAccessModifier : ISymbolModifier
{
    public BufferAccess Access { get; }

    public BufferAccessModifier(BufferAccess access)
    {
        Access = access;
    }
}
using SPSL.Language.Core;

namespace SPSL.Language.Symbols.Modifiers;

public class BufferStorageModifier : ISymbolModifier
{
    public BufferStorage Storage { get; }

    public BufferStorageModifier(BufferStorage storage)
    {
        Storage = storage;
    }
}
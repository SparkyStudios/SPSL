using SPSL.Language.Parsing.Common;

namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class BufferStorageModifier : ISymbolModifier
{
    public BufferStorage Storage { get; }

    public BufferStorageModifier(BufferStorage storage)
    {
        Storage = storage;
    }
}
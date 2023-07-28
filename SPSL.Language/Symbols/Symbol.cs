namespace SPSL.Language.Symbols;

public class Symbol
{
    public SymbolType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public int Start { get; init; } = -1;

    public int End { get; init; } = -1;

    public IEnumerable<ISymbolModifier> Modifiers { get; init; } = Array.Empty<ISymbolModifier>();
}
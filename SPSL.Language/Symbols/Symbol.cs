using SPSL.Language.Symbols.Modifiers;

namespace SPSL.Language.Symbols;

public class Symbol
{
    public SymbolType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public int Start { get; init; } = -1;

    public int End { get; init; } = -1;

    public IEnumerable<ISymbolModifier> Modifiers { get; init; } = Array.Empty<ISymbolModifier>();

    public SymbolTable? Parent { get; internal set; }

    public bool IsFileSymbol => Start == -1 && End == -1;

    #region Methods

    public Reference ToReference()
    {
        return new()
        {
            Source = Source,
            Start = Start,
            End = End
        };
    }
    
    #endregion
}
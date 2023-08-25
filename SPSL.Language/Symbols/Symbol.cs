using SPSL.Language.Symbols.Modifiers;

namespace SPSL.Language.Symbols;

public class Symbol
{
    public SymbolType Type { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Source { get; init; } = string.Empty;

    public int Start { get; init; } = -1;

    public int End { get; init; } = -1;

    public IEnumerable<ISymbolModifier> Modifiers { get; init; } = Enumerable.Empty<ISymbolModifier>();

    public SymbolTable? Parent { get; internal set; }
    
    public string Key { get; internal set; } = string.Empty;
    
    public ICollection<Reference> References { get; internal set; } = new List<Reference>();

    public bool IsFileSymbol => Start == -1 && End == -1;

    #region Methods

    public Reference ToReference(string? source = null, int? start = null, int? end = null)
    {
        return new()
        {
            Source = source ?? Source,
            Start = start ?? Start,
            End = end ?? End
        };
    }
    
    #endregion
}
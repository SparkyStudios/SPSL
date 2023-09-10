using SPSL.Language.Analysis.Symbols.Modifiers;
using SPSL.Language.Symbols;

namespace SPSL.Language.Analysis.Symbols;

public class Symbol : IEquatable<Symbol>
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

    #region Overrides

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Symbol)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Type, Name, Modifiers);
    }

    #endregion

    #region IEquatable<Symbol> Implementation

    public bool Equals(Symbol? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type && Name == other.Name && Modifiers.Equals(other.Modifiers);
    }

    #endregion
}
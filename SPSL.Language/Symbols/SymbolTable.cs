using System.Collections;

namespace SPSL.Language.Symbols;

public class SymbolTable : Symbol
{
    private readonly Hashtable _symbols = new();

    public void Add(Symbol symbol)
    {
        _symbols.Add(symbol.Name, symbol);
    }

    public Symbol Lookup(string name)
    {
        return _symbols[name] as Symbol ??
               throw new InvalidOperationException($"Could not find symbol '{name}' in the table.");
    }

    public bool Contains(string name)
    {
        return _symbols.ContainsKey(name);
    }
}
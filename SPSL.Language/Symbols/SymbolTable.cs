using System.Collections;

namespace SPSL.Language.Symbols;

public class SymbolTable : Symbol
{
    #region Fields

    private readonly Hashtable _symbols = new();

    private readonly Dictionary<string, int> _keysCounter = new();

    #endregion

    #region Properties

    public IEnumerable<Symbol> Symbols => _symbols.Values.Cast<Symbol>();

    #endregion

    private string GetSymbolKey(string name)
    {
        if (_keysCounter.TryGetValue(name, out int value))
        {
            _keysCounter[name] = ++value;
            return $"{name}@{value}";
        }

        _keysCounter[name] = 0;
        return name;
    }

    /// <summary>
    /// Adds a symbol in the current table.
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    public void Add(Symbol symbol)
    {
        symbol.Key = GetSymbolKey(symbol.Name);
        _symbols.Add(symbol.Key, symbol);
        symbol.Parent = this;
    }

    /// <summary>
    /// Search for a symbol in this table. This is not recursive.
    /// </summary>
    /// <param name="name">The name of the symbol</param>
    /// <returns>The found symbol.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Symbol? Lookup(string name)
    {
        return _symbols[name] as Symbol;
    }

    /// <summary>
    /// Checks if the symbol with the given name exists in the table.
    /// </summary>
    /// <param name="name">The name to check for.</param>
    /// <returns>
    /// <c>true</c> if a symbol with that name has been found, <c>false</c> otherwise.
    /// </returns>
    public bool Contains(string name)
    {
        return _symbols.ContainsKey(name);
    }

    public Symbol? Resolve(string source, int position)
    {
        Symbol? foundSymbol = null;

        foreach (var item in _symbols.Values)
        {
            switch (item)
            {
                case SymbolTable table:
                    foundSymbol = table.Resolve(source, position);
                    break;
                case Symbol { IsFileSymbol: false } symbol:
                {
                    if (symbol.Source == source && symbol.Start <= position && symbol.End >= position)
                        foundSymbol = symbol;
                    break;
                }
            }

            if (foundSymbol is not null) break;
        }

        return foundSymbol;
    }
}
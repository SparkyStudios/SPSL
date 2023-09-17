using System.Collections;
using SPSL.Language.Analysis.Common;

namespace SPSL.Language.Analysis.Symbols;

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

    public Symbol? Resolve(string name)
    {
        SymbolTable? currentScope = this;

        while (currentScope is not null)
        {
            if (currentScope.Contains(name))
                return currentScope.Lookup(name);

            // TODO: This DOESNT fetch the correct override, but instead the first one.
            var foundFunction = currentScope.Symbols.OfType<SymbolTable>()
                .Where(table => table.Type == SymbolType.Function && table.Symbols.Any(symbol =>
                    symbol.Type == SymbolType.Identifier && symbol.Name == name));

            if (foundFunction.Any())
                return foundFunction.First().Resolve(name);

            currentScope = currentScope.Parent;
        }

        return null;
    }

    public SymbolTable FindEnclosingScope(string source, int position)
    {
        SymbolTable currentScope = this;

        var enclosingScopes = currentScope.Symbols.OfType<SymbolTable>()
            .Where(s => s.IsFileSymbol || (s.Source == source && s.Start <= position && s.End >= position))
            .OrderByDescending(t => t.Start)
            .ToArray();

        foreach (SymbolTable enclosingScope in enclosingScopes)
        {
            currentScope = enclosingScope.FindEnclosingScope(source, position);
            if (!ReferenceEquals(currentScope, enclosingScope) && !currentScope.IsFileSymbol)
                break;
        }

        return currentScope;
    }

    public SymbolTable Merge(SymbolTable right)
    {
        foreach (Symbol symbol in right.Symbols)
        {
            Symbol? existingSymbol = Lookup(symbol.Name);

            // If a symbol with the same name and the same type already exists in the table, we should skip it.
            if (existingSymbol is not null)
            {
                if (symbol.Type == existingSymbol.Type)
                {
                    // If the symbol table exists, merge its contents
                    if (existingSymbol is SymbolTable t)
                        t.Merge((symbol as SymbolTable)!);

                    continue;
                }
            }

            Add(symbol);
        }

        return this;
    }

    public Symbol? LookupInCurrentAndChildTables(string name)
    {
        Symbol? symbol = Lookup(name);

        if (symbol != null)
            return symbol;

        // Check all children tables recursively 
        foreach (SymbolTable childTable in Symbols.OfType<SymbolTable>())
        {
            symbol = childTable.LookupInCurrentAndChildTables(name);
            if (symbol != null)
                return symbol;
        }

        return null;
    }
}
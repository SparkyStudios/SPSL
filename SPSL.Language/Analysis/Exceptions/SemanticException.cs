using SPSL.Language.Analysis.Symbols;

namespace SPSL.Language.Analysis.Exceptions;

public class SemanticException : Exception
{
    public enum SemanticExceptionType
    {
        DuplicateSymbol,
        SymbolNotDeclared,
        ContextKeywordUsedOutOfContext
    }

    public Symbol Symbol { get; }

    public SemanticExceptionType Type { get; }

    public SemanticException(string message, SemanticExceptionType type, Symbol symbol)
        : base($"[{type}] ({symbol.Source}): {message}")
    {
        Type = type;
        Symbol = symbol;
    }
}
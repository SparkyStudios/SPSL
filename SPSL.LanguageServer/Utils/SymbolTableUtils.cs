using OmniSharp.Extensions.LanguageServer.Protocol;
using SPSL.Language.Analysis.Symbols;

namespace SPSL.LanguageServer.Utils;

internal static class SymbolTableUtils
{
    public static SymbolTable GetParentSymbolTableAtOffset(DocumentUri uri, SymbolTable root, int offset)
    {
        while (true)
        {
            SymbolTable currentSymbolTable = root;

            foreach (Symbol item in root.Symbols)
            {
                if (item is not SymbolTable symbol || symbol.Source != uri.ToString()) continue;

                int start = item.Start;
                int end = item.End;

                if (!currentSymbolTable.IsFileSymbol &&
                    (start < currentSymbolTable.Start || end > currentSymbolTable.End)) continue;
                if (currentSymbolTable.IsFileSymbol || (start <= offset && offset <= end)) currentSymbolTable = symbol;
            }

            if (currentSymbolTable == root) return root;
            root = currentSymbolTable;
        }
    }
}
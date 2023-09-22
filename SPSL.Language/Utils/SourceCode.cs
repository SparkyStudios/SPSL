using Antlr4.Runtime;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Analysis.Visitors;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Visitors;

namespace SPSL.Language.Utils;

public static class SourceCode
{
    private enum ParseFileMode
    {
        Shader,
        Material
    }

    private static void ParseDirectory
    (
        string p,
        IEnumerable<string> libs,
        HashSet<NamespacedReference> importedNamespaces,
        out Ast ast,
        out SymbolTable symbolTable
    )
    {
        ast = new();
        symbolTable = new();

        IEnumerable<string> paths = libs as string[] ?? libs.ToArray();

        foreach (string libraryPath in paths.Select(Path.GetFullPath))
        {
            if (!Directory.Exists(libraryPath))
                continue;

            foreach (string file in Directory.GetFiles(Path.Join(libraryPath, p), "*.spsl*",
                         SearchOption.AllDirectories))
            {
                string ns = Path.GetDirectoryName(file)![(libraryPath.Length + 1)..]
                    .Replace(Path.DirectorySeparatorChar.ToString(), Namespace.Separator);

                int pos = ns.LastIndexOf(Namespace.Separator, StringComparison.Ordinal);

                ParseFileMode mode = file.EndsWith(".spslm", StringComparison.Ordinal)
                    ? ParseFileMode.Material
                    : ParseFileMode.Shader;

                ParseFile(mode, file, paths, importedNamespaces, out Ast fileAst, out SymbolTable fileSymbolTable);

                if (pos >= 0)
                {
                    string parent = ns[..pos];

                    if (ast.FirstOrDefault(n => n.FullName == parent) is { } parentNode)
                    {
                        fileAst[ns].Name = new() { Value = ns[(pos + Namespace.SeparatorLength)..] };
                        fileAst[ns].ParentNamespace = parentNode;

                        if (parentNode.Namespaces.FirstOrDefault(n => n.FullName == fileAst[ns].FullName) is
                            { } parentNamespace)
                            parentNamespace.Merge(fileAst[ns]);
                        else
                            parentNode.Children.Add(fileAst[ns]);
                    }
                }

                importedNamespaces.Add(new(ns));
                ast.Merge(fileAst);
                symbolTable.Merge(fileSymbolTable);
            }
        }
    }

    private static void ParseFile
    (
        ParseFileMode mode,
        string p,
        IEnumerable<string> libs,
        HashSet<NamespacedReference> importedNamespaces,
        out Ast ast,
        out SymbolTable symbolTable
    )
    {
        using var spsl = new StreamReader(p);
        IEnumerable<string> paths = libs as string[] ?? libs.ToArray();

        // ---- Build AST

        SPSLLexer lexer = new(new AntlrInputStream(spsl));
        lexer.RemoveErrorListeners();

        SPSLParser parser = new(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();

        AstVisitor shaderVisitor = new(p);
        ast = shaderVisitor.Visit(mode == ParseFileMode.Shader ? parser.shaderFile() : parser.materialFile());

        parser.Reset();

        SymbolVisitor symbolVisitor = new(p);
        symbolTable =
            (SymbolTable)symbolVisitor.Visit(mode == ParseFileMode.Shader
                ? parser.shaderFile()
                : parser.materialFile());

        foreach (NamespacedReference import in shaderVisitor.Imports.Where(i => !importedNamespaces.Contains(i)))
        {
            ParseDirectory
            (
                Path.Join(import.Name.Split(Namespace.Separator)),
                paths,
                importedNamespaces,
                out Ast directoryAst,
                out SymbolTable directorySymbolTable
            );

            ast.Merge(directoryAst);
            symbolTable.Merge(directorySymbolTable);

            importedNamespaces.Add(import);
        }
    }

    public static void Shader(string path, IEnumerable<string> libraryPaths, out Ast ast, out SymbolTable symbolTable)
    {
        HashSet<NamespacedReference> importedNamespaces = new();
        ParseFile(ParseFileMode.Shader, path, libraryPaths, importedNamespaces, out ast, out symbolTable);
    }

    public static void Material(string path, IEnumerable<string> libraryPaths, out Ast ast, out SymbolTable symbolTable)
    {
        HashSet<NamespacedReference> importedNamespaces = new();
        ParseFile(ParseFileMode.Material, path, libraryPaths, importedNamespaces, out ast, out symbolTable);
    }
}
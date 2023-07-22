using Antlr4.Runtime;
using System.Collections;
using SPSL.Language.Visitors;

namespace SPSL.Language.AST;

public class AST : IEnumerable<Namespace>
{
    private class NamespaceNameComparer : IComparer<KeyValuePair<string, Namespace>>
    {
        public int Compare(KeyValuePair<string, Namespace> x, KeyValuePair<string, Namespace> y)
        {
            string l = x.Key;
            string r = y.Key;
            int f = -1;

            if (x.Key.Length < y.Key.Length)
            {
                l = y.Key;
                r = x.Key;
                f = 1;
            }

            return l.StartsWith(r) ? f : l.CompareTo(r);
        }
    }

    private Dictionary<string, Namespace> _namespaces = new();

    public Namespace this[string name] => _namespaces[name];

    private enum ParseFileMode
    {
        Shader,
        Material
    }

    private static AST ParseDirectory(string p, IEnumerable<string> libs, HashSet<string> importedNamespaces)
    {
        AST ast = new();
        IEnumerable<string> paths = libs as string[] ?? libs.ToArray();

        foreach (var libraryPath in paths.Select(Path.GetFullPath))
        {
            if (!Directory.Exists(libraryPath))
                continue;

            foreach (var file in Directory.GetFiles(Path.Join(libraryPath, p), "*.spsl*",
                         SearchOption.AllDirectories))
            {
                var ns = Path.GetDirectoryName(file)![(libraryPath.Length + 1)..]
                    .Replace(Path.DirectorySeparatorChar.ToString(), "::");
                var pos = ns.LastIndexOf("::", StringComparison.Ordinal);

                var mode = file.EndsWith(".spslm", StringComparison.Ordinal) ? ParseFileMode.Material : ParseFileMode.Shader;
                var parsed = ParseFile(mode, file, paths, importedNamespaces);

                if (pos >= 0)
                {
                    var parent = ns[..pos];

                    if (ast.FirstOrDefault(n => n.FullName == parent) is { } parentNode)
                    {
                        parsed[ns].Name = ns[(pos + 2)..];
                        parsed[ns].Parent = parentNode;

                        if (parentNode.Namespaces.FirstOrDefault(n => n.FullName == parsed[ns].FullName) is
                            { } parentNamespace)
                            parentNamespace.Merge(parsed[ns]);
                        else
                            parentNode.Children.Add(parsed[ns]);
                    }
                }

                importedNamespaces.Add(ns);
                ast.Merge(parsed);
            }
        }

        return ast;
    }

    private static AST ParseFile(ParseFileMode mode, string p, IEnumerable<string> libs, HashSet<string> importedNamespaces)
    {
        using var spsl = new StreamReader(p);
        IEnumerable<string> paths = libs as string[] ?? libs.ToArray();

        // ---- Build AST

        SPSLLexer lexer = new(new AntlrInputStream(spsl));

        lexer.RemoveErrorListeners();

        SPSLParser parser = new(new CommonTokenStream(lexer));
        parser.RemoveErrorListeners();

        ASTVisitor shaderVisitor = new();

        AST ast = shaderVisitor.Visit(mode == ParseFileMode.Shader ? parser.shaderFile() : parser.materialFile());

        foreach (var import in shaderVisitor.Imports.Where(i => !importedNamespaces.Contains(i)))
        {
            ast.Merge(ParseDirectory(Path.Join(import.Split("::")), paths, importedNamespaces));
            importedNamespaces.Add(import);
        }

        return ast;
    }

    public static AST FromShaderFile(string path, IEnumerable<string> libraryPaths)
    {
        HashSet<string> importedNamespaces = new();
        return ParseFile(ParseFileMode.Shader, path, libraryPaths, importedNamespaces);
    }

    public static AST FromMaterialFile(string path, IEnumerable<string> libraryPaths)
    {
        HashSet<string> importedNamespaces = new();
        return ParseFile(ParseFileMode.Material, path, libraryPaths, importedNamespaces);
    }

    public AST AddNamespace(Namespace ns)
    {
        if (_namespaces.TryGetValue(ns.FullName, out var found))
            found.Merge(ns);
        else
            _namespaces.Add(ns.FullName, ns);

        _namespaces = _namespaces.Order(new NamespaceNameComparer()).ToDictionary(name => name.Key, value => value.Value);

        return this;
    }

    public IEnumerator<Namespace> GetEnumerator()
    {
        return _namespaces.Values.GetEnumerator();
    }

    public Namespace? GetNamespace(string name)
    {
        return _namespaces.TryGetValue(name, out var found) ? found : null;
    }

    public AST Merge(AST ast)
    {
        foreach (var ns in ast._namespaces)
            AddNamespace(ns.Value);

        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
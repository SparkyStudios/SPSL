using Antlr4.Runtime;
using System.Collections;
using SPSL.Language.Visitors;

namespace SPSL.Language.AST;

public class Ast : IEnumerable<Namespace>
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

            return l.StartsWith(r) ? f : string.Compare(l, r, StringComparison.Ordinal);
        }
    }

    private Dictionary<string, Namespace> _namespaces = new();

    public Namespace this[string name] => _namespaces[name];

    private enum ParseFileMode
    {
        Shader,
        Material
    }

    private static Ast ParseDirectory
    (
        string p,
        IEnumerable<string> libs,
        HashSet<string> importedNamespaces
    )
    {
        Ast ast = new();
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

                Ast parsed = ParseFile(mode, file, paths, importedNamespaces);

                if (pos >= 0)
                {
                    string parent = ns[..pos];

                    if (ast.FirstOrDefault(n => n.FullName == parent) is { } parentNode)
                    {
                        parsed[ns].Name = new() { Value = ns[(pos + Namespace.SeparatorLength)..] };
                        parsed[ns].ParentNamespace = parentNode;

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

    private static Ast ParseFile
    (
        ParseFileMode mode,
        string p,
        IEnumerable<string> libs,
        HashSet<string> importedNamespaces
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

        Ast ast = shaderVisitor.Visit(mode == ParseFileMode.Shader ? parser.shaderFile() : parser.materialFile());

        foreach (string import in shaderVisitor.Imports.Where(i => !importedNamespaces.Contains(i)))
        {
            ast.Merge(ParseDirectory(Path.Join(import.Split(Namespace.Separator)), paths, importedNamespaces));
            importedNamespaces.Add(import);
        }

        return ast;
    }

    public static Ast FromShaderFile(string path, IEnumerable<string> libraryPaths)
    {
        HashSet<string> importedNamespaces = new();
        return ParseFile(ParseFileMode.Shader, path, libraryPaths, importedNamespaces);
    }

    public static Ast FromMaterialFile(string path, IEnumerable<string> libraryPaths)
    {
        HashSet<string> importedNamespaces = new();
        return ParseFile(ParseFileMode.Material, path, libraryPaths, importedNamespaces);
    }

    public Ast AddNamespace(Namespace ns)
    {
        if (_namespaces.TryGetValue(ns.FullName, out Namespace? found))
            found.Merge(ns);
        else
            _namespaces.Add(ns.FullName, ns);

        _namespaces = _namespaces.Order(new NamespaceNameComparer())
            .ToDictionary(name => name.Key, value => value.Value);

        return this;
    }

    public IEnumerator<Namespace> GetEnumerator()
    {
        return _namespaces.Values.GetEnumerator();
    }

    public Namespace? GetNamespace(string name)
    {
        return _namespaces.TryGetValue(name, out Namespace? found) ? found : null;
    }

    public Ast Merge(Ast ast)
    {
        foreach (var ns in ast._namespaces)
            AddNamespace(ns.Value);

        return this;
    }

    public INode? ResolveNode(string source, int offset)
    {
        return _namespaces.Values.FirstOrDefault(ns => ns.ResolveNode(source, offset) is not null)
            ?.ResolveNode(source, offset);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
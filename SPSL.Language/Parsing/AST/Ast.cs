using System.Collections;
using SPSL.Language.Core;

namespace SPSL.Language.Parsing.AST;

public class Ast : ILanguageFeature<Ast>, IEnumerable<Namespace>
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

    public Namespace this[NamespacedReference name] => _namespaces[name];

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
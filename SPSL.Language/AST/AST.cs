using System.Collections;

namespace SPSL.Language.AST;

public class AST : IEnumerable<Namespace>
{
    private readonly Dictionary<string, Namespace> _namespaces = new();

    public Namespace this[string name] => _namespaces[name];

    public AST AddNamespace(Namespace ns)
    {
        if (_namespaces.TryGetValue(ns.FullName, out var found))
            found.Merge(ns);
        else
            _namespaces.Add(ns.FullName, ns);

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
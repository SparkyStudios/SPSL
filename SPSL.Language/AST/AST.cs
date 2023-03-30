using System.Collections;

namespace SPSL.Language.AST;

public class AST : IEnumerable<Namespace>
{
    private Dictionary<string, Namespace> _namespaces = new Dictionary<string, Namespace>();

    public Namespace this[string name]
    {
        get => _namespaces[name];
    }

    public AST AddNamespace(Namespace ns)
    {
        if (_namespaces.ContainsKey(ns.FullName))
            _namespaces[ns.FullName].Merge(ns);
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
        if (_namespaces.ContainsKey(name))
            return _namespaces[name];

        return null;
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
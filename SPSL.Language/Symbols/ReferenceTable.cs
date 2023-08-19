using System.Collections;

namespace SPSL.Language.Symbols;

public class ReferenceTable
{
    private readonly Hashtable _references = new();

    public void Add(string name, Reference reference)
    {
        if (Contains(name))
        {
            var s = Lookup(name);
            s.Add(reference);
            return;
        }

        _references.Add(name, new HashSet<Reference> { reference });
    }

    public HashSet<Reference> Lookup(string name)
    {
        return _references[name] as HashSet<Reference> ??
               throw new InvalidOperationException($"Could not find reference '{name}' in the table.");
    }

    public bool Contains(string name)
    {
        return _references.ContainsKey(name);
    }
}


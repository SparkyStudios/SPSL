using System.Collections;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL namespace.
/// </summary>
public class Namespace : INamespaceChild, IEnumerable<INamespaceChild>
{
    #region Fields

    private HashSet<Namespace> _importedNamespaces;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the root namespace in which this namespace is a child.
    /// </summary>
    public Namespace Root => Parent == null ? this : Parent.Root;

    /// <summary>
    /// Gets the fully qualified name of the namespace.
    /// </summary>
    public string FullName
    {
        get
        {
            var fullName = Name;

            if (Parent != null)
                fullName = $"{Parent.FullName}/{fullName}";

            return fullName;
        }
    }

    /// <summary>
    /// Gets the list of namespace imported to this namespace.
    /// </summary>
    public IReadOnlySet<Namespace> Imports => _importedNamespaces;

    /// <summary>
    /// Gets all registered children of this namespace.
    /// </summary>
    public HashSet<INamespaceChild> Children { get; }

    /// <summary>
    /// The collection of child <see cref="Language.AST.Namespace"/>s of
    /// this one.
    /// </summary>
    public IEnumerable<Namespace> Namespaces => Children.OfType<Namespace>();

    /// <summary>
    /// The collection of <see cref="Type"/>s declared
    /// in this namespace.
    /// </summary>
    public IEnumerable<Type> Types => Children.OfType<Type>();

    /// <summary>
    /// The collection of <see cref="Interface"/>s declared
    /// in this namespace.
    /// </summary>
    public IEnumerable<Interface> Interfaces => Children.OfType<Interface>();

    /// <summary>
    /// The collection of <see cref="ShaderFragment"/>s
    /// declared in this namespace.
    /// </summary>
    public IEnumerable<ShaderFragment> ShaderFragments => Children.OfType<ShaderFragment>();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="Language.AST.Namespace"/>.
    /// </summary>
    /// <param name="name">The name of this namespace.</param>
    public Namespace(string name)
    {
        _importedNamespaces = new HashSet<Namespace>();

        Name = name;
        Children = new HashSet<INamespaceChild>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Merges types, interfaces, and fragments declared in the
    /// <paramref name="other"/> <see cref="Language.AST.Namespace"/> in this one.
    /// </summary>
    /// <param name="other">The other namespace to merge into this one.</param>
    public void Merge(Namespace other)
    {
        foreach (INamespaceChild child in other.Children)
            Children.Add(child);
    }

    /// <summary>
    /// Register a child into this namespace.
    /// </summary>
    /// <param name="child">The child to register.</param>
    public void AddChild(INamespaceChild child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    /// <summary>
    /// Gets a <see cref="INamespaceChild"/> from this namespace using the given name.
    /// </summary>
    /// <remarks>
    /// The name can be namespaced. If so, this method will look up in the child
    /// namespaces of this namespace.
    /// </remarks>
    /// <param name="name">The namespaced name of the <see cref="INamespaceChild"/> to retrieve.</param>
    /// <returns>The child of this namespace who matches the given <paramref name="name"/>, or null if not found.</returns>
    public INamespaceChild? GetChild(string name)
    {
        // If the name is namespaced
        if (name.Contains('/'))
        {
            var pos = name.IndexOf('/');
            var nsName = name[..pos];

            foreach (Namespace ns in Namespaces.Where(ns => ns.Name == nsName))
                return ns.GetChild(name[(pos + 1)..]);
        }
        else
        {
            foreach (INamespaceChild child in Children.Where(child => child.Name == name))
                return child;
        }

        // Try in imported namespaces
        return Imports.Select(ns => ns.GetChild(name)).FirstOrDefault();
    }

    /// <summary>
    /// Adds the given namespace reference as an import for this namespace. This will made types
    /// from this namespace available in the current namespace.
    /// </summary>
    /// <param name="ns">The namespace to import in this one.</param>
    public void Use(Namespace ns)
    {
        if (ns == this)
            return;

        _importedNamespaces.Add(ns);
    }

    public override string ToString()
    {
        return FullName;
    }

    public IEnumerator<INamespaceChild> GetEnumerator()
    {
        return ((IEnumerable<INamespaceChild>)Children).GetEnumerator();
    }

    #endregion

    #region IEnumerable<INamespaceChild> Implementation

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Children).GetEnumerator();
    }

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Language.AST.Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? Parent { get; set; }

    /// <summary>
    /// The namespace's name.
    /// </summary>
    public string Name { get; set; }

    #endregion
}
using System.Collections;
using System.Diagnostics;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an SPSL namespace.
/// </summary>
[DebuggerDisplay("{FullName,nq}")]
public class Namespace : INamespaceChild, IEnumerable<INamespaceChild>
{
    #region Properties

    public const char SeparatorChar = ':';

    public const string Separator = "::";

    public const int SeparatorLength = 2;

    /// <summary>
    /// Gets the root namespace in which this namespace is a child.
    /// </summary>
    public Namespace Root => ParentNamespace == null ? this : ParentNamespace.Root;

    /// <summary>
    /// Gets the fully qualified name of the namespace.
    /// </summary>
    public string FullName
    {
        get
        {
            string fullName = Name.Value;

            if (ParentNamespace != null)
                fullName = $"{ParentNamespace.FullName}{Separator}{fullName}";

            return fullName;
        }
    }

    /// <summary>
    /// Gets all registered children of this namespace.
    /// </summary>
    public HashSet<INamespaceChild> Children { get; }

    /// <summary>
    /// The collection of child <see cref="Namespace"/>s of
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
    /// Initializes a new instance of <see cref="Namespace"/>.
    /// </summary>
    /// <param name="name">The name of this namespace.</param>
    public Namespace(Identifier name)
    {
        name.Parent = this;

        Name = name;
        Children = new();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Merges types, interfaces, and fragments declared in the
    /// <paramref name="other"/> <see cref="Namespace"/> in this one.
    /// </summary>
    /// <param name="other">The other namespace to merge into this one.</param>
    public void Merge(Namespace other)
    {
        foreach (INamespaceChild child in other.Children)
        {
            child.ParentNamespace = this;
            Children.Add(child);
        }
    }

    /// <summary>
    /// Register a child into this namespace.
    /// </summary>
    /// <param name="child">The child to register.</param>
    public INamespaceChild AddChild(INamespaceChild child)
    {
        child.ParentNamespace = this;
        Children.Add(child);

        return child;
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
        if (name.Contains(Separator))
        {
            int pos = name.IndexOf(Separator, StringComparison.Ordinal);
            string nsName = name[..pos];

            foreach (Namespace ns in Namespaces.Where(ns => ns.Name.Value == nsName))
                return ns.GetChild(name[(pos + SeparatorLength)..]);
        }

        return Children.FirstOrDefault(child => child.Name.Value == name);
    }

    public INode? ResolveNode(string source, int offset)
    {
        INode? found = null;

        int closestStart = -1;
        int closestEnd = -1;

        foreach (INamespaceChild child in Children)
        {
            if (child.Source != source || child.Start > offset || child.End < offset ||
                (closestStart != -1 && child.Start <= closestStart) ||
                (closestEnd != -1 && child.End >= closestEnd)) continue;

            found = child;
            closestStart = child.Start;
            closestEnd = child.End;
        }

        return found?.ResolveNode(source, offset);
    }
    
    public IEnumerator<INamespaceChild> GetEnumerator()
    {
        return ((IEnumerable<INamespaceChild>)Children).GetEnumerator();
    }

    #endregion

    #region IEnumerable<INamespaceChild> Implementation

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Children).GetEnumerator();
    }

    #endregion

    #region INamespaceChild Implementation

    /// <inheritdoc cref="INamespaceChild.ParentNamespace"/>
    public Namespace? ParentNamespace { get; set; }

    /// <inheritdoc cref="INamespaceChild.Name"/>
    public Identifier Name { get; set; }

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    #endregion
}
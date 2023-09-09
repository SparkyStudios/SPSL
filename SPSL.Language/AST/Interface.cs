namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL shader interface.
/// </summary>
public class Interface : INamespaceChild
{
    #region Properties

    /// <summary>
    /// The list of interfaces that this interface extends.
    /// </summary>
    public HashSet<NamespacedReference> ExtendedInterfaces { get; }

    /// <summary>
    /// The list of methods declared in this interface.
    /// </summary>
    public HashSet<FunctionHead> Methods { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Interface" /> class.
    /// </summary>
    /// <param name="name">The name of the interface.</param>
    public Interface(Identifier name)
    {
        name.Parent = this;

        Name = name;
        ExtendedInterfaces = new();
        Methods = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Interface" /> class.
    /// </summary>
    /// <param name="name">The name of the interface.</param>
    /// <param name="extendedInterfaces">The list of extended interfaces.</param>
    /// <param name="methods">The list of methods implemented by this interface.</param>
    public Interface
    (
        Identifier name,
        IEnumerable<NamespacedReference> extendedInterfaces,
        IEnumerable<FunctionHead> methods
    )
    {
        Name = name;
        ExtendedInterfaces = new(extendedInterfaces);
        Methods = new(methods);

        Name.Parent = this;

        foreach (NamespacedReference extendedInterface in ExtendedInterfaces)
            extendedInterface.Parent = this;

        foreach (FunctionHead functionHead in Methods)
            functionHead.Parent = this;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Extends the interface with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the interface to extend.</param>
    public void Extends(NamespacedReference name)
    {
        name.Parent = this;
        ExtendedInterfaces.Add(name);
    }

    /// <summary>
    /// Makes this interface implement the given <paramref name="method"/>.
    /// </summary>
    /// <param name="method">The method to implement.</param>
    public void Implements(FunctionHead method)
    {
        method.Parent = this;
        Methods.Add(method);
    }

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Language.AST.Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? ParentNamespace { get; set; }

    /// <summary>
    /// The namespace's name.
    /// </summary>
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

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return ExtendedInterfaces.FirstOrDefault(e => e.ResolveNode(source, offset) is not null)
                   ?.ResolveNode(source, offset) ??
               Methods.FirstOrDefault(e => e.ResolveNode(source, offset) is not null)
                   ?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL shader interface.
/// </summary>
public class Interface : INamespaceChild, IDocumented
{
    #region Properties

    public HashSet<NamespacedReference> ExtendedInterfaces { get; }

    public HashSet<FunctionHead> FunctionHeads { get; }

    #endregion

    #region Constructors

    public Interface(Identifier name)
    {
        name.Parent = this;

        Name = name;
        ExtendedInterfaces = new();
        FunctionHeads = new();
    }

    public Interface
    (
        Identifier name,
        IEnumerable<NamespacedReference> extendedInterfaces,
        IEnumerable<FunctionHead> functionHeads
    )
    {
        Name = name;
        ExtendedInterfaces = new(extendedInterfaces);
        FunctionHeads = new(functionHeads);

        Name.Parent = this;

        foreach (NamespacedReference extendedInterface in ExtendedInterfaces)
            extendedInterface.Parent = this;

        foreach (FunctionHead functionHead in FunctionHeads)
            functionHead.Parent = this;
    }

    #endregion

    #region Methods

    public void AddExtendedInterface(NamespacedReference v)
    {
        v.Parent = this;
        ExtendedInterfaces.Add(v);
    }

    public void AddFunctionHead(FunctionHead functionHead)
    {
        functionHead.Parent = this;
        FunctionHeads.Add(functionHead);
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
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return ExtendedInterfaces.FirstOrDefault(e => e.ResolveNode(source, offset) is not null)
                   ?.ResolveNode(source, offset) ??
               FunctionHeads.FirstOrDefault(e => e.ResolveNode(source, offset) is not null)
                   ?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
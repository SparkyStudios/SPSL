namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL shader interface.
/// </summary>
public class Interface : INamespaceChild, INode
{
    #region Properties

    public HashSet<NamespacedReference> ExtendedInterfaces { get; }

    public HashSet<FunctionHead> FunctionHeads { get; }

    #endregion

    #region Constructors

    public Interface(string name)
    {
        Name = name;
        ExtendedInterfaces = new HashSet<NamespacedReference>();
        FunctionHeads = new HashSet<FunctionHead>();
    }

    public Interface(string name, IEnumerable<NamespacedReference> extendedInterfaces,
        IEnumerable<FunctionHead> functionHeads)
    {
        Name = name;
        ExtendedInterfaces = new HashSet<NamespacedReference>(extendedInterfaces);
        FunctionHeads = new HashSet<FunctionHead>(functionHeads);
    }

    #endregion

    #region Methods

    public void AddExtendedInterface(string v)
    {
        AddExtendedInterface(new NamespacedReference(v));
    }

    public void AddExtendedInterface(NamespacedReference v)
    {
        ExtendedInterfaces.Add(v);
    }

    public void AddFunctionHead(FunctionHead functionHead)
    {
        FunctionHeads.Add(functionHead);
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

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
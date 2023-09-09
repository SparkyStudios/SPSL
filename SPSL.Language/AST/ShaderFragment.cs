using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL shader fragment. A set of functions
/// which can be used directly into a shader.
/// </summary>
public class ShaderFragment : INamespaceChild, IBlock
{
    #region Properties

    public NamespacedReference ExtendedShaderFragment { get; init; }

    public OrderedSet<NamespacedReference> ExtendedInterfaces { get; }

    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    public IEnumerable<GlobalVariable> GlobalVariables => Children.OfType<GlobalVariable>();

    public IEnumerable<PermutationVariable> PermutationVariables => Children.OfType<PermutationVariable>();

    public IEnumerable<StructuredBuffer> StructuredBuffers => Children.OfType<StructuredBuffer>();

    public IEnumerable<TypedBuffer> TypedBuffers => Children.OfType<TypedBuffer>();

    public IEnumerable<Type> Types => Children.OfType<Type>();

    public IEnumerable<Stream> Streams => Children.OfType<Stream>();

    public IEnumerable<ShaderFunction> Functions => Children.OfType<ShaderFunction>();

    #endregion

    #region Constructors

    public ShaderFragment(Identifier name)
    {
        name.Parent = this;

        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new();
        Children = new();
    }

    public ShaderFragment(Identifier name, IEnumerable<ShaderFunction> functions)
    {
        name.Parent = this;

        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new();
        Children = new(functions);

        foreach (IBlockChild function in Children)
            function.Parent = this;
    }

    public ShaderFragment(Identifier name, params ShaderFunction[] functions)
    {
        name.Parent = this;

        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new();
        Children = new(functions);

        foreach (IBlockChild function in Children)
            function.Parent = this;
    }

    public ShaderFragment
    (
        Identifier name,
        IEnumerable<ShaderFunction> functions,
        IEnumerable<NamespacedReference> interfaces
    )
    {
        name.Parent = this;

        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new(interfaces);
        Children = new(functions);

        foreach (NamespacedReference reference in ExtendedInterfaces)
            reference.Parent = this;

        foreach (IBlockChild function in Children)
            function.Parent = this;
    }

    #endregion

    #region Methods

    public void Extends(NamespacedReference @interface)
    {
        @interface.Parent = this;
        ExtendedInterfaces.Add(@interface);
    }

    public void Extends(Identifier @interface)
    {
        @interface.Parent = this;
        Extends(new NamespacedReference(@interface));
    }

    public void Uses(NamespacedReference name)
    {
        name.Parent = this;
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (var name in names)
            Uses(name);
    }

    public void AddGlobalVariable(GlobalVariable variable)
    {
        variable.Parent = this;
        Children.Add(variable);
    }

    public void AddPermutationVariable(PermutationVariable permutation)
    {
        permutation.Parent = this;
        Children.Add(permutation);
    }

    public void AddBuffer(StructuredBuffer buffer)
    {
        buffer.Parent = this;
        Children.Add(buffer);
    }

    public void AddFunction(ShaderFunction function)
    {
        function.Parent = this;
        Children.Add(function);
    }

    public void AddShaderMember(IShaderMember member)
    {
        member.Parent = this;
        Children.Add(member);
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

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; }

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
        return Name.ResolveNode(source, offset) ??
               Children.FirstOrDefault(x => x.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
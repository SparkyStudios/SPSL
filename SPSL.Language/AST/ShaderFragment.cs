using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL shader fragment. A set of functions
/// which can be used directly into a shader.
/// </summary>
public class ShaderFragment : INamespaceChild, IBlock
{
    #region Properties

    public NamespacedReference ExtendedShaderFragment { get; set; } = NamespacedReference.Null;

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

    public ShaderFragment(string name)
    {
        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new OrderedSet<NamespacedReference>();
        Children = new OrderedSet<IBlockChild>();
    }

    public ShaderFragment(string name, IEnumerable<ShaderFunction> functions)
    {
        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new OrderedSet<NamespacedReference>();
        Children = new OrderedSet<IBlockChild>(functions.Cast<IBlockChild>());
    }

    public ShaderFragment(string name, params ShaderFunction[] functions)
    {
        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new OrderedSet<NamespacedReference>();
        Children = new OrderedSet<IBlockChild>(functions.Cast<IBlockChild>());
    }

    public ShaderFragment(string name, IEnumerable<ShaderFunction> functions,
        IEnumerable<NamespacedReference> interfaces)
    {
        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new OrderedSet<NamespacedReference>(interfaces);
        Children = new OrderedSet<IBlockChild>(functions.Cast<IBlockChild>());
    }

    #endregion

    #region Methods

    public void Extends(NamespacedReference @interface)
    {
        ExtendedInterfaces.Add(@interface);
    }

    public void Extends(string @interface)
    {
        ExtendedInterfaces.Add(new NamespacedReference(@interface));
    }

    public void Uses(NamespacedReference name)
    {
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (var name in names)
            Uses(name);
    }

    public void AddGlobalVariable(GlobalVariable variable)
    {
        Children.Add(variable);
    }

    public void AddPermutationVariable(PermutationVariable permutation)
    {
        Children.Add(permutation);
    }

    public void AddBuffer(StructuredBuffer buffer)
    {
        Children.Add(buffer);
    }

    public void AddFunction(ShaderFunction function)
    {
        Children.Add(function);
    }

    public void AddShaderMember(IShaderMember member)
    {
        Children.Add(member);
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

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
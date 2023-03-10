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

    public ShaderFragment(string name, IEnumerable<ShaderFunction> functions, IEnumerable<NamespacedReference> interfaces)
    {
        Name = name;
        ExtendedShaderFragment = NamespacedReference.Null;
        ExtendedInterfaces = new OrderedSet<NamespacedReference>(interfaces);
        Children = new OrderedSet<IBlockChild>(functions.Cast<IBlockChild>());
    }

    #endregion

    #region Methods

    public void AddExtendedInterface(NamespacedReference @interface)
    {
        ExtendedInterfaces.Add(@interface);
    }

    public void AddExtendedInterface(string @interface)
    {
        ExtendedInterfaces.Add(new NamespacedReference(@interface));
    }

    public void AddFunction(ShaderFunction function)
    {
        Children.Add(function);
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
}
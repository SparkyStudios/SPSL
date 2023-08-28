using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Shader : IBlock, INamespaceChild
{
    #region Nested Types

    public struct ComputeShaderParams
    {
        public uint ThreadCountX;
        public uint ThreadCountY;
        public uint ThreadCountZ;
    }

    #endregion

    #region Properties

    public bool IsAbstract { get; set; }

    public ShaderStage Stage { get; }

    public NamespacedReference ExtendedShader { get; init; } = NamespacedReference.Null;

    public HashSet<NamespacedReference> Interfaces { get; } = new();

    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    public ComputeShaderParams ComputeParams { get; }

    #endregion

    #region Constructors

    public Shader(ShaderStage stage, Identifier name)
    {
        name.Parent = this;

        Stage = stage;
        Name = name;
    }

    public Shader(Identifier name, ComputeShaderParams @params)
    {
        name.Parent = this;

        Stage = ShaderStage.Compute;
        Name = name;
        ComputeParams = @params;
    }

    #endregion

    #region Public Methods

    public void Implements(NamespacedReference name)
    {
        name.Parent = this;
        Interfaces.Add(name);
    }

    public void Implements(IEnumerable<NamespacedReference> names)
    {
        foreach (NamespacedReference name in names)
            Implements(name);
    }

    public void Uses(NamespacedReference name)
    {
        name.Parent = this;
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (NamespacedReference name in names)
            Uses(name);
    }

    #endregion

    #region IBlock Implementation

    public OrderedSet<IBlockChild> Children { get; } = new();

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
        return ExtendedShader.ResolveNode(source, offset) ??
               Interfaces.FirstOrDefault(i => i.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               ImportedShaderFragments.FirstOrDefault(i => i.ResolveNode(source, offset) != null)
                   ?.ResolveNode(source, offset) ??
               Children.FirstOrDefault(c => c.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               Name.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
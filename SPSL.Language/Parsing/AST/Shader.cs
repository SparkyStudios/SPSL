using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

public class Shader : IBlock, INamespaceChild, ISemanticallyEquatable
{
    #region Nested Types

    /// <summary>
    /// Template parameters for a compute shader.
    /// </summary>
    public struct ComputeShaderParams
    {
        /// <summary>
        /// The thread group size on the X axis.
        /// </summary>
        public uint ThreadCountX { get; init; }

        /// <summary>
        /// The thread group size on the Y axis.
        /// </summary>
        public uint ThreadCountY { get; init; }

        /// <summary>
        /// The thread group size on the Z axis.
        /// </summary>
        public uint ThreadCountZ { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputeShaderParams"/> struct.
        /// </summary>
        public ComputeShaderParams()
        {
            ThreadCountX = 1;
            ThreadCountY = 1;
            ThreadCountZ = 1;
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Whether the shader is an abstract shader.
    /// </summary>
    public bool IsAbstract { get; init; }

    /// <summary>
    /// The shader stage.
    /// </summary>
    public ShaderStage Stage { get; }

    /// <summary>
    /// The references to the shader that this shader depends on.
    /// </summary>
    public NamespacedReference ExtendedShader { get; init; } = NamespacedReference.Null;

    /// <summary>
    /// The list of interfaces that this shader implements.
    /// </summary>
    public HashSet<NamespacedReference> Interfaces { get; } = new();

    /// <summary>
    /// The list of shader fragments that this shader imports.
    /// </summary>
    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    /// <summary>
    /// The compute shaders parameters owned by this shader. This value is useful
    /// only when the shader is a compute shader.
    /// </summary>
    public ComputeShaderParams ComputeParams { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Shader"/> class.
    /// </summary>
    /// <param name="stage">The shader stage.</param>
    /// <param name="name">The shader name.</param>
    public Shader(ShaderStage stage, Identifier name)
    {
        name.Parent = this;

        Stage = stage;
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Shader"/> class. This will assume that
    /// the shader stage is <see cref="ShaderStage.Compute"/>.
    /// </summary>
    /// <param name="name">The shader name.</param>
    /// <param name="params">The compute shader parameters.</param>
    public Shader(Identifier name, ComputeShaderParams @params)
        : this(ShaderStage.Compute, name)
    {
        ComputeParams = @params;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Makes this shader implement the specified interface.
    /// </summary>
    /// <param name="name">The interface name.</param>
    public void Implements(NamespacedReference name)
    {
        name.Parent = this;
        Interfaces.Add(name);
    }

    /// <summary>
    /// Makes this shader implement the specified list of interfaces.
    /// </summary>
    /// <param name="names">The list of interfaces.</param>
    public void Implements(IEnumerable<NamespacedReference> names)
    {
        foreach (NamespacedReference name in names)
            Implements(name);
    }

    /// <summary>
    /// Makes this shader import the specified shader fragment.
    /// </summary>
    /// <param name="name">The shader fragment name.</param>
    public void Uses(NamespacedReference name)
    {
        name.Parent = this;
        ImportedShaderFragments.Add(name);
    }

    /// <summary>
    /// Makes this shader import the specified list of shader fragments.
    /// </summary>
    /// <param name="names">The list of shader fragments.</param>
    public void Uses(IEnumerable<NamespacedReference> names)
    {
        foreach (NamespacedReference name in names)
            Uses(name);
    }

    #endregion

    #region IBlock Implementation

    /// <inheritdoc cref="IBlock.Children"/>
    public OrderedSet<IBlockChild> Children { get; } = new();

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

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not Shader other) return false;

        // Two shaders are semantically equal if they have the same stage and same name.
        return Stage == other.Stage && Name.SemanticallyEquals(other.Name);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Stage, Name.GetSemanticHashCode());
    }

    #endregion
}
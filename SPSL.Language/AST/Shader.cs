using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Shader : INamespaceChild, IBlock
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

    public NamespacedReference ExtendedShader { get; set; } = NamespacedReference.Null;

    public HashSet<NamespacedReference> Interfaces { get; } = new();

    public OrderedSet<NamespacedReference> ImportedShaderFragments { get; } = new();

    public ComputeShaderParams ComputeParams { get; }

    #endregion

    #region Constructors

    public Shader(ShaderStage stage, string name)
    {
        Stage = stage;
        Name = name;
    }

    public Shader(string name, ComputeShaderParams @params)
    {
        Stage = ShaderStage.Compute;
        Name = name;
        ComputeParams = @params;
    }

    #endregion

    #region Public Methods

    public void Implements(NamespacedReference name)
    {
        Interfaces.Add(name);
    }

    public void Implements(IEnumerable<NamespacedReference> names)
    {
        foreach (var name in names)
            Implements(name);
    }

    public void Uses(NamespacedReference name)
    {
        ImportedShaderFragments.Add(name);
    }

    public void Uses(IEnumerable<NamespacedReference> names)
    {
        ImportedShaderFragments.AddRange(names);
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
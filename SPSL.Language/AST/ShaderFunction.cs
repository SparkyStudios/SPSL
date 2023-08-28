using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class ShaderFunction : IAnnotated, IShaderMember, IMaterialMember
{
    #region Properties

    public bool IsOverride { get; init; }

    public Function Function { get; }

    public bool IsConstructor { get; init; }

    #endregion

    #region Constructors

    public ShaderFunction(Function function)
    {
        function.Parent = this;

        Annotations = new();
        IsOverride = false;
        Function = function;
    }

    #endregion

    #region IAnnotated Implementation

    public OrderedSet<Annotation> Annotations { get; }

    #endregion

    #region IBlockChild Implementation

    public Identifier Name
    {
        get => ((IBlockChild)Function).Name;
        set => ((IBlockChild)Function).Name = value;
    }

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
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Function.ResolveNode(source, offset) ??
               Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
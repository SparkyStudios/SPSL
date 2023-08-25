using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a function inside a struct.
/// </summary>
public class TypeFunction : IAnnotated, IDocumented, IBlockChild
{
    #region Properties

    public Function Function { get; }

    #endregion

    #region Construtors

    public TypeFunction(Function function)
    {
        Function = function;
    }

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations"/>
    public OrderedSet<Annotation> Annotations { get; init; } = new();

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

    #endregion

    #region IBlockChild Implementation

    public Identifier Name
    {
        get => ((IBlockChild)Function).Name;
        set => ((IBlockChild)Function).Name = value;
    }

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
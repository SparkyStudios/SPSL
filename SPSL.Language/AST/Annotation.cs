using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL annotation.
/// </summary>
/// <example>
/// <code>@anAnnotation(val1, val2, ...)</code>
/// </example>
public class Annotation : INode
{
    #region Properties

    /// <summary>
    /// The name of the annotation.
    /// </summary>
    public Identifier Identifier { get; init; } = null!;

    /// <summary>
    /// The arguments of the annotation.
    /// </summary>
    public OrderedSet<IExpression> Arguments { get; init; } = new();

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Identifier.ResolveNode(source, offset) ??
               Arguments.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
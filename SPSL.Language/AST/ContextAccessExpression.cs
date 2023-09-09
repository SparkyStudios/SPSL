namespace SPSL.Language.AST;

/// <summary>
/// Represents an access to an SPSL context.
/// </summary>
public class ContextAccessExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The target context.
    /// </summary>
    /// <value>
    /// Can be either "this" or "base".
    /// </value>
    public Identifier Target { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextAccessExpression"/> class.
    /// </summary>
    /// <param name="target">The target context.</param>
    public ContextAccessExpression(Identifier target)
    {
        target.Parent = this;

        Target = target;
    }

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
        return Target.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
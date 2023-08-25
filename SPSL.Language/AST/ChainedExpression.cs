namespace SPSL.Language.AST;

/// <summary>
/// Represents a chained methods/properties expression.
/// </summary>
public class ChainedExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The targeted object in which the member is accessed.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// The list of members of this chained expression.
    /// </summary>
    public List<IExpression> Members { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMemberReferenceExpression"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="members">The list of members in the chained expression.</param>
    public ChainedExpression(IExpression target, IEnumerable<IExpression> members)
    {
        target.Parent = this;

        Target = target;
        Members = new(members);

        foreach (IExpression member in Members)
            member.Parent = this;
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
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Target.ResolveNode(source, offset) ??
               Members.FirstOrDefault(m => m.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
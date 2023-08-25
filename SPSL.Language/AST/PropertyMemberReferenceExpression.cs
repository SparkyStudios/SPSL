namespace SPSL.Language.AST;

/// <summary>
/// Represents an access to a shader field.
/// </summary>
public class PropertyMemberReferenceExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The targeted object in which the member is accessed.
    /// </summary>
    public Identifier Target { get; }

    /// <summary>
    /// The field name in the target object.
    /// </summary>
    public Identifier Member { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMemberReferenceExpression"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="member">The member to access.</param>
    public PropertyMemberReferenceExpression(Identifier target, Identifier member)
    {
        target.Parent = this;
        member.Parent = this;
        
        Target = target;
        Member = member;
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
        return Target.ResolveNode(source, offset) ?? Member.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
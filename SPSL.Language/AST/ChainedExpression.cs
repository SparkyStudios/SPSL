namespace SPSL.Language.AST;

/// <summary>
/// Represent a method call through a target shader.
/// </summary>
public class ChainedExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The targeted object in which the member is accessed.
    /// </summary>
    public IExpression Target { get; set; }

    /// <summary>
    /// The list of members of this chained expression.
    /// </summary>
    public List<IExpression> Members { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMemberReferenceExpression"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="members">The list of members in the chained expression.</param>
    public ChainedExpression(IExpression target, IEnumerable<IExpression> members)
    {
        Target = target;
        Members = new(members);
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
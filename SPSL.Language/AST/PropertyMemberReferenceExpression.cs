namespace SPSL.Language.AST;

/// <summary>
/// Represent an access to a shader field.
/// </summary>
public class PropertyMemberReferenceExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The targeted object in which the member is accessed.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The field name in the target object.
    /// </summary>
    public string Member { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMemberReferenceExpression"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="member">The member to access.</param>
    public PropertyMemberReferenceExpression(string target, string member)
    {
        Target = target;
        Member = member;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
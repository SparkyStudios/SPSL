namespace SPSL.Language.AST;

/// <summary>
/// Represent a method call through a target shader.
/// </summary>
public class MethodMemberReferenceExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The targeted object in which the member is accessed.
    /// </summary>
    public string Target { get; set; }

    /// <summary>
    /// The field name in the target object.
    /// </summary>
    public InvocationExpression Member { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="PropertyMemberReferenceExpression"/>.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="member">The method call expression.</param>
    public MethodMemberReferenceExpression(string target, InvocationExpression member)
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
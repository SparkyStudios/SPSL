namespace SPSL.Language.AST;

public class ContextAccessExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The target context.
    /// </summary>
    /// <value>
    /// Can be either "this" or "base".
    /// </value>
    public string Target { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextAccessExpression"/> class.
    /// </summary>
    /// <param name="target">The target context.</param>
    public ContextAccessExpression(string target)
    {
        Target = target;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
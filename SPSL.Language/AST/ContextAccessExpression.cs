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

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
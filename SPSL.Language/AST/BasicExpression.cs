namespace SPSL.Language.AST;

/// <summary>
/// Represents the most basic expression in SPSL.
/// It generally reference an existing variable.
/// </summary>
public class BasicExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The expression's identifier.
    /// </summary>
    public string Identifier { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="BasicExpression"/>.
    /// </summary>
    /// <param name="identifier">The identifier of the expression.</param>
    public BasicExpression(string identifier)
    {
        Identifier = identifier;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;
    
    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
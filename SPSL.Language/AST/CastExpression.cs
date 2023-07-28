namespace SPSL.Language.AST;

/// <summary>
/// Represent a value cast expression.
/// </summary>
public class CastExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The type to cast to.
    /// </summary>
    public IDataType Type { get; set; }

    /// <summary>
    /// The expression to cast.
    /// </summary>
    public IExpression Expression { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="CastExpression"/>.
    /// </summary>
    /// <param name="type">The type to cast to.</param>
    /// <param name="expression">The expression to cast into the given <paramref name="type"/>.</param>
    public CastExpression(IDataType type, IExpression expression)
    {
        Type = type;
        Expression = expression;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
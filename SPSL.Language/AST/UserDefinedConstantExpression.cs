namespace SPSL.Language.AST;

/// <summary>
/// Represent an user defined constant expression.
/// </summary>
public class UserDefinedConstantExpression : IConstantExpression
{
    #region Properties

    /// <summary>
    /// The reference to the constant value identifier.
    /// </summary>
    public NamespacedReference Identifier { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UserDefinedConstantExpression"/>.
    /// </summary>
    /// <param name="identifier">The reference to the constant value identifier.</param>
    public UserDefinedConstantExpression(NamespacedReference identifier)
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
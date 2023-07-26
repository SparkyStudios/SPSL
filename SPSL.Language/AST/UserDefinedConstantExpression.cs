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

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
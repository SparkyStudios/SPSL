namespace SPSL.Language.AST;

/// <summary>
/// Represents a literal value.
/// </summary>
public interface ILiteral : IPrimitiveExpression
{
    #region Properties

    /// <summary>
    /// The value wrapped by this expression.
    /// </summary>
    object Value { get; set; }

    #endregion
}
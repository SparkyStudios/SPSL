namespace SPSL.Language.AST;

/// <summary>
/// Represent a double value.
/// </summary>
public class DoubleLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public double Value
    {
        get => (double)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="DoubleLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public DoubleLiteral(double value)
    {
        Value = value;
    }

    #endregion
}
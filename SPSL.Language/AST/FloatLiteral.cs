namespace SPSL.Language.AST;

/// <summary>
/// Represent a float value.
/// </summary>
public class FloatLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public float Value
    {
        get => (float)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="FloatLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public FloatLiteral(float value)
    {
        Value = value;
    }

    #endregion
}
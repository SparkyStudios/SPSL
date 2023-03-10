namespace SPSL.Language.AST;

/// <summary>
/// Represent an integer value.
/// </summary>
public class IntegerLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public int Value
    {
        get => (int)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="IntegerLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public IntegerLiteral(int value)
    {
        Value = value;
    }

    #endregion
}
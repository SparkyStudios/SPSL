namespace SPSL.Language.AST;

/// <summary>
/// Represent a boolean value.
/// </summary>
public class BoolLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public bool Value
    {
        get => (bool)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="BoolLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public BoolLiteral(bool value)
    {
        Value = value;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
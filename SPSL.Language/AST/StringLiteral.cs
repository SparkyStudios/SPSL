namespace SPSL.Language.AST;

/// <summary>
/// Represent a string value.
/// </summary>
public class StringLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public string Value
    {
        get => (string)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="StringLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public StringLiteral(string value)
    {
        Value = value;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
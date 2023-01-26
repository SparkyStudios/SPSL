namespace SPSL.Language.AST;

/// <summary>
/// Represent a unsigned integer value.
/// </summary>
public class UnsignedIntegerLiteral : ILiteral
{
    #region Properties

    /// <summary>
    /// The value.
    /// </summary>
    public uint Value
    {
        get => (uint)((ILiteral)this).Value;
        set => ((ILiteral)this).Value = value;
    }

    #endregion

    #region ILiteral Implementation

    /// <inheritdoc cref="ILiteral.Value"/>
    object ILiteral.Value { get; set; } = null!;

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="UnsignedIntegerLiteral"/>.
    /// </summary>
    /// <param name="value">The value literal.</param>
    public UnsignedIntegerLiteral(uint value)
    {
        Value = value;
    }

    #endregion
}
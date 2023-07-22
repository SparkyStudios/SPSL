namespace SPSL.Serialization.Reflection;

public enum TextureAddressMode
{
    /// <summary>
    /// No wrapping. It is equivalent to use BorderColor wrapping with black.
    /// </summary>
    None = 0,

    /// <summary>
    /// The texture is repeated.
    /// </summary>
    Repeat,

    /// <summary>
    /// The texture is clamped with the edge colors.
    /// </summary>
    ClampToEdge,

    /// <summary>
    /// The texture is clamped with a custom border color.
    /// </summary>
    BorderColor,

    /// <summary>
    /// The texture is repeated with a mirror pattern.
    /// </summary>
    MirroredRepeat,

    Default = Repeat
}

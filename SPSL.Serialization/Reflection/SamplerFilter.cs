namespace SPSL.Serialization.Reflection;

public enum SamplerFilter
{
    /// <summary>
    /// Ignore filtering.
    /// </summary>
    None,

    /// <summary>
    /// Point sampling.
    /// </summary>
    Point,

    /// <summary>
    /// Linear sampling.
    /// </summary>
    Linear,

    Default = Linear
}

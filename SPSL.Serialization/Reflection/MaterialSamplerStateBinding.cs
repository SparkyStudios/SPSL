using System.Diagnostics;
using System.Runtime.Serialization;

namespace SPSL.Serialization.Reflection;

/// <summary>
/// Binding to a sampler.
/// </summary>
[DataContract]
[DebuggerDisplay("SamplerState {Key}")]
public class MaterialSamplerStateBinding
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialSamplerStateBinding"/> class.
    /// </summary>
    /// <param name="keyName">Name of the key.</param>
    /// <param name="description">The description.</param>
    public MaterialSamplerStateBinding(string keyName, SamplerStateDescription description)
    {
        KeyName = keyName;
        Description = description;
    }

    /// <summary>
    /// The key name.
    /// </summary>
    [DataMember(Order = 0)]
    public string KeyName;

    /// <summary>
    /// The description of this sampler.
    /// </summary>
    [DataMember(Order = 1)]
    public SamplerStateDescription Description;
}

using System.Collections;
using System.Runtime.Serialization;
using MessagePack;
using SPSL.Language.Parsing.Common;

namespace SPSL.Serialization.Reflection;

/// <summary>
/// Reflects material data from a binary representation.
/// </summary>
[DataContract]
public class MaterialReflection
{
    /// <summary>
    /// The material name.
    /// </summary>
    [DataMember(Order = 0)]
    public string Name { get; }

    /// <summary>
    /// The list of input attributes. An input attribute is represented by its semantic and its index.
    /// </summary>
    [DataMember(Order = 1)]
    public ICollection<InputElementDescription> InputElements { get; } = new List<InputElementDescription>();

    /// <summary>
    /// The list of samplers created in the material.
    /// </summary>
    [DataMember(Order = 2)]
    public ICollection<MaterialSamplerStateBinding> Samplers { get; } = new List<MaterialSamplerStateBinding>();

    /// <summary>
    /// The shader byte code.
    /// </summary>
    [DataMember(Order = 3)]
    public ShaderByteCode ShaderByteCode { get; } = new();

    /// <summary>
    /// The name of entry points per shader stage.
    /// </summary>
    [DataMember(Order = 4)]
    public Dictionary<ShaderStage, string> EntryPoints { get; } = new();

    /// <summary>
    /// The list of permutation values for this material.
    /// </summary>
    [DataMember(Order = 5)]
    public Hashtable Permutations { get; } = new();
    
    /// <summary>
    /// The list of material states as provided in the SPSL code.
    /// </summary>
    [DataMember(Order = 6)]
    public ICollection<MaterialStateBinding> States { get; } = new List<MaterialStateBinding>();
    
    public MaterialReflection(string name)
    {
        Name = name;
    }

    public void Serialize(string filePath)
    {
        using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        MessagePackSerializer.Serialize(stream, this);
    }
}
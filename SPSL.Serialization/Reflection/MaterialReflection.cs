using System.Runtime.Serialization;
using MessagePack;
using SPSL.Language.AST;
using SPSL.Language.Core;

namespace SPSL.Serialization.Reflection;

[DataContract]
public class MaterialReflection
{
    [DataMember(Order = 0)]
    public string Name { get; } = string.Empty;

    [DataMember(Order = 1)]
    public IEnumerable<InputAttributeDescription> InputAttributes { get; } = new List<InputAttributeDescription>();

    [DataMember(Order = 2)]
    public IEnumerable<MaterialSamplerStateBinding> Samplers { get; } = new List<MaterialSamplerStateBinding>();

    [DataMember(Order = 3)]
    public ShaderByteCode ShaderByteCode { get; } = new();

    [DataMember(Order = 4)]
    public Dictionary<ShaderStage, string> EntryPoints { get; } = new();

    public MaterialReflection(string name)
    {
        Name = name;
    }

    public void Serialize(string filePath)
    {
        using var stream = System.IO.File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream);

        MessagePackSerializer.SerializeToJson(writer, this);
    }
}

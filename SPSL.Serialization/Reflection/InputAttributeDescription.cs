using System.Runtime.Serialization;

namespace SPSL.Serialization.Reflection;

[DataContract]
public struct InputAttributeDescription
{
    [DataMember(Order = 0)]
    public string SemanticName { get; set; }

    [DataMember(Order = 1)]
    public uint SemanticIndex { get; set; }

    [DataMember(Order = 2)]
    public InputAttributeFormat Format { get; set; }
}

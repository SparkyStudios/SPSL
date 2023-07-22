using System.Numerics;
using System.Runtime.Serialization;

namespace SPSL.Serialization.Reflection;

[DataContract]
public struct SamplerStateDescription
{
    [DataMember(Order = 0)]
    private readonly Dictionary<string, object> _properties = new();

    public SamplerStateDescription()
    {
        _properties["BorderColor"] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);

        _properties["AddressModeS"] = TextureAddressMode.Default;
        _properties["AddressModeT"] = TextureAddressMode.Default;
        _properties["AddressModeR"] = TextureAddressMode.Default;

        _properties["MinFilter"] = SamplerFilter.Default;
        _properties["MagFilter"] = SamplerFilter.Default;
        _properties["MipFilter"] = SamplerFilter.Default;

        _properties["LodBias"] = 0.0f;
        _properties["MinLod"] = 0;
        _properties["MaxLod"] = uint.MaxValue;

        _properties["MaxAnisotropy"] = (byte)0;
    }

    [IgnoreDataMember]
    public Vector4 BorderColor
    {
        get => (Vector4)_properties.GetValueOrDefault("BorderColor", new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        set => _properties["BorderColor"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeS
    {
        get => (TextureAddressMode)_properties.GetValueOrDefault("AddressModeS", TextureAddressMode.Default);
        set => _properties["AddressModeS"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeT
    {
        get => (TextureAddressMode)_properties.GetValueOrDefault("AddressModeT", TextureAddressMode.Default);
        set => _properties["AddressModeT"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeR
    {
        get => (TextureAddressMode)_properties.GetValueOrDefault("AddressModeR", TextureAddressMode.Default);
        set => _properties["AddressModeR"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MinFilter
    {
        get => (SamplerFilter)_properties.GetValueOrDefault("MinFilter", SamplerFilter.Default);
        set => _properties["MinFilter"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MagFilter
    {
        get => (SamplerFilter)_properties.GetValueOrDefault("MagFilter", SamplerFilter.Default);
        set => _properties["MagFilter"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MipFilter
    {
        get => (SamplerFilter)_properties.GetValueOrDefault("MipFilter", SamplerFilter.Default);
        set => _properties["MipFilter"] = value;
    }

    [IgnoreDataMember]
    public float LodBias
    {
        get => (float)_properties.GetValueOrDefault("LodBias", 0.0f);
        set => _properties["LodBias"] = value;
    }

    [IgnoreDataMember]
    public uint MinLod
    {
        get => (uint)_properties.GetValueOrDefault("MinLod", 0);
        set => _properties["MinLod"] = value;
    }

    [IgnoreDataMember]
    public uint MaxLod
    {
        get => (uint)_properties.GetValueOrDefault("MaxLod", uint.MaxValue);
        set => _properties["MaxLod"] = value;
    }

    [IgnoreDataMember]
    public byte MaxAnisotropy
    {
        get => (byte)_properties.GetValueOrDefault("MaxAnisotropy", 0);
        set => _properties["MaxAnisotropy"] = value;
    }
}

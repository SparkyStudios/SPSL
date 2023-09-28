using System.Collections;
using System.Numerics;
using System.Runtime.Serialization;

namespace SPSL.Serialization.Reflection;

[DataContract]
public readonly struct SamplerStateDescription
{
    [DataMember(Order = 0)] private readonly Hashtable _properties = new();

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
        get => _properties["BorderColor"] as Vector4? ?? new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        set => _properties["BorderColor"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeS
    {
        get => _properties["AddressModeS"] as TextureAddressMode? ?? TextureAddressMode.Default;
        set => _properties["AddressModeS"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeT
    {
        get => _properties["AddressModeT"] as TextureAddressMode? ?? TextureAddressMode.Default;
        set => _properties["AddressModeT"] = value;
    }

    [IgnoreDataMember]
    public TextureAddressMode AddressModeR
    {
        get => _properties["AddressModeR"] as TextureAddressMode? ?? TextureAddressMode.Default;
        set => _properties["AddressModeR"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MinFilter
    {
        get => _properties["MinFilter"] as SamplerFilter? ?? SamplerFilter.Default;
        set => _properties["MinFilter"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MagFilter
    {
        get => _properties["MagFilter"] as SamplerFilter? ?? SamplerFilter.Default;
        set => _properties["MagFilter"] = value;
    }

    [IgnoreDataMember]
    public SamplerFilter MipFilter
    {
        get => _properties["MipFilter"] as SamplerFilter? ?? SamplerFilter.Default;
        set => _properties["MipFilter"] = value;
    }

    [IgnoreDataMember]
    public float LodBias
    {
        get => _properties["LodBias"] as float? ?? 0.0f;
        set => _properties["LodBias"] = value;
    }

    [IgnoreDataMember]
    public uint MinLod
    {
        get => _properties["MinLod"] as uint? ?? 0;
        set => _properties["MinLod"] = value;
    }

    [IgnoreDataMember]
    public uint MaxLod
    {
        get => _properties["MaxLod"] as uint? ?? uint.MaxValue;
        set => _properties["MaxLod"] = value;
    }

    [IgnoreDataMember]
    public byte MaxAnisotropy
    {
        get => _properties["MaxAnisotropy"] as byte? ?? 0;
        set => _properties["MaxAnisotropy"] = value;
    }
}
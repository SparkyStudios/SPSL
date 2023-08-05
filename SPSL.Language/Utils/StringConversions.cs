using SPSL.Language.Core;

namespace SPSL.Language.Utils;

public static class StringConversions
{
    public static BufferStorage ToBufferStorage(this string storage)
    {
        return storage switch
        {
            "coherent" => BufferStorage.Coherent,
            _ => BufferStorage.Undefined
        };
    }

    public static BufferAccess ToBufferAccess(this string access)
    {
        return access switch
        {
            "readonly" => BufferAccess.ReadOnly,
            "writeonly" => BufferAccess.WriteOnly,
            "readwrite" => BufferAccess.ReadWrite,
            "constant" => BufferAccess.Constant,
            _ => BufferAccess.ReadOnly
        };
    }

    public static ShaderStage ToShaderStage(this string stage)
    {
        return stage switch
        {
            "vertex" => ShaderStage.Vertex,
            "fragment" => ShaderStage.Pixel,
            "pixel" => ShaderStage.Pixel,
            "geometry" => ShaderStage.Geometry,
            "hull" => ShaderStage.Hull,
            "domain" => ShaderStage.Domain,
            "compute" => ShaderStage.Compute,
            _ => ShaderStage.Unknown,
        };
    }
}
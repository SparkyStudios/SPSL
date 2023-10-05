namespace SPSL.Serialization;

/// <summary>
/// Specifies in which language the SPSL code has been translated.
/// </summary>
public enum ShaderSourceLanguage : byte
{
    HLSL = 1,
    GLSL = 2,
    MTL = 3
}
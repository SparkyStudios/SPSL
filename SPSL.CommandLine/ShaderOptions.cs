using CommandLine;

namespace SPSL.CommandLine;

[Verb("shader", HelpText = "Compiles SPSL shader into HLSL or Vulkan GLSL source code.")]
public class ShaderOptions : ShadingOptions
{
}
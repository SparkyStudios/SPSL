using CommandLine;

namespace SPSL.CommandLine;

[Verb("shader", HelpText = "Compiles SPSL shader into HLSL or Vulkan GLSL source code.")]
public class ShaderOptions : ShadingOptions
{
    [Option('n', "shaders", HelpText = "The list of shaders to compile. This is useful when you want to compile only a set of shaders from the input file and the imported libraries. You should specify the full name per shader, with its namespace.")]
    public IEnumerable<string> Shaders { get; set; }
}
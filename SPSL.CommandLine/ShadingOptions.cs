using CommandLine;

namespace SPSL.CommandLine;

public abstract class ShadingOptions : BaseOptions
{
    [Option('g', "generator", HelpText = "Specifies the shader generator to use.", Default = ShaderSourceGenerator.HLSL, Required = false)]
    public ShaderSourceGenerator Generator { get; set; } = ShaderSourceGenerator.HLSL;

    [Option('p', "permutations", HelpText = "Specifies the permutations to apply to the shader code.", Required = false, Separator = ',')]
    public IEnumerable<string> Permutations { get; set; } = Enumerable.Empty<string>();
}
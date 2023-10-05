using CommandLine;
using SPSL.CommandLine.Utils;
using SPSL.Serialization;

namespace SPSL.CommandLine;

public abstract class ShadingOptions : BaseOptions
{
    [Option('g', "generator", HelpText = "Specifies the shader generator to use.", Default = ShaderSourceLanguage.HLSL, Required = false)]
    public ShaderSourceLanguage Language { get; set; } = ShaderSourceLanguage.HLSL;

    [Option('p', "permutations", HelpText = "Create a variant by explicitly specifying a set of permutation values.", Required = false, Separator = ',', SetName = "Variants")]
    public IEnumerable<PermutationValue> Permutations { get; set; } = Enumerable.Empty<PermutationValue>();
    
    [Option('m', "generate-variants", HelpText = "Instructs the compiler to generate all the possible variants. A file with the extension .variants.json will be created, and will contain the list the list of generated variant names and its permutation values.", Required = false, SetName = "Variants")]
    public bool GenerateVariants { get; set; } = false;
}
using CommandLine;

namespace SPSL.CommandLine;

public class PermutationValue
{
    public string Name { get; }
    public string Value { get; }

    public PermutationValue(string option)
    {
        string[] parts = option.Split('=', 2);

        Name = parts[0];
        Value = parts[1];
    }

    public override string ToString()
    {
        return $"{Name}={Value}";
    }
}

public abstract class ShadingOptions : BaseOptions
{
    [Option('g', "generator", HelpText = "Specifies the shader generator to use.", Default = ShaderSourceGenerator.HLSL, Required = false)]
    public ShaderSourceGenerator Generator { get; set; } = ShaderSourceGenerator.HLSL;

    [Option('p', "permutations", HelpText = "Specifies the permutations to apply to the shader code.", Required = false, Separator = ',')]
    public IEnumerable<PermutationValue> Permutations { get; set; } = Enumerable.Empty<PermutationValue>();
}
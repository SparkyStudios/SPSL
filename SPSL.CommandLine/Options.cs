using CommandLine;

namespace SPSL.CommandLine;

public enum Generator
{
    HLSL,
    GLSL
}

public class Options
{
    [Option(HelpText = "Specifies whether to generate shader code.", Default = false, Required = false, Group = "mode")]
    public bool Shader { get; set; }

    [Option("node-graph", HelpText = "Specifies whether to generate node graph binary.", Default = false, Required = false, Group = "mode")]
    public bool NodeGraph { get; set; }

    [Option('i', "input", HelpText = "Specifies the input file path. For shaders, specifies the path to a .spsls file. For node graphs, specifies the path to a .spslg file.", Required = false)]
    public string InputFile { get; set; } = string.Empty;

    [Option('o', "output", HelpText = "Specifies the output directory path.", Required = false)]
    public string OutputDirectory { get; set; } = string.Empty;

    [Option('l', "lib", HelpText = "Specifies the library directory path.", Required = false, Min = 1)]
    public IEnumerable<string> LibDirectories { get; set; } = null!;

    [Option("std-lib", HelpText = "Specifies whether the SPSL standard library is used.", Default = true, Required = false)]
    public bool UseStandardLib { get; set; } = true;

    [Option('g', "generator", HelpText = "Specifies the code generator to use. HLSL and GLSL will generate shader code", Default = Generator.HLSL, Required = false)]
    public Generator Generator { get; set; } = Generator.HLSL;

    [Option('v', "verbose", HelpText = "Set output to verbose messages.", Required = false)]
    public bool Verbose { get; set; } = false;
}

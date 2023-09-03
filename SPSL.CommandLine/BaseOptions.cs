using CommandLine;

namespace SPSL.CommandLine;

public abstract class BaseOptions
{
    [Option('v', "verbose", HelpText = "Set output to verbose messages.", Required = false)]
    public bool Verbose { get; set; } = false;
    
    [Option('i', "input", HelpText = "Specifies the input file path. For shaders, specifies the path to a .spsls file. For node graphs, specifies the path to a .spslg file.", Required = false)]
    public string InputFile { get; set; } = string.Empty;

    [Option('o', "output", HelpText = "Specifies the output directory path.", Required = false)]
    public string OutputDirectory { get; set; } = string.Empty;

    [Option('l', "lib", HelpText = "Specifies the library directory path.", Required = false, Min = 1)]
    public IEnumerable<string> LibDirectories { get; set; } = null!;

    [Option("no-std-lib", HelpText = "Specifies whether the SPSL standard library should not be used.", Default = false, Required = false)]
    public bool NoStandardLib { get; set; } = false;
}
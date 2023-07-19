using CommandLine;
using SPSL.CommandLine;
using SPSL.Language.AST;
using SPSL.Translation.HLSL;

using Parser = CommandLine.Parser;

void SetupParser(ParserSettings settings)
{
    settings.AllowMultiInstance = false;
    settings.AutoHelp = true;
    settings.AutoVersion = true;
    settings.CaseSensitive = true;
    settings.HelpWriter = Console.Out;
}

void RunOptions(Options opts)
{
    if (opts is { Shader: true, NodeGraph: true })
    {
        Console.Error.WriteLine("Cannot specify both --shader and --node-graph.");
        Environment.Exit(1);
    }

    if (string.IsNullOrEmpty(opts.InputFile))
    {
        Console.Error.WriteLine("No input file specified.");
        Environment.Exit(1);
    }

    if (!File.Exists(opts.InputFile))
    {
        Console.Error.WriteLine($"Input file '{opts.InputFile}' does not exist.");
        Environment.Exit(1);
    }

    if (string.IsNullOrEmpty(opts.OutputDirectory))
    {
        opts.OutputDirectory = Path.GetDirectoryName(Path.GetFullPath(opts.InputFile))!;
    }

    if (!Directory.Exists(opts.OutputDirectory))
    {
        Console.Error.WriteLine($"Output directory '{opts.OutputDirectory}' does not exist.");
        Environment.Exit(1);
    }

    if (opts.Shader)
    {
        // Build AST
        var ast = AST.FromFile(opts.InputFile, opts.LibDirectories);

        switch (opts.Generator)
        {
            case Generator.GLSL:
                Console.Error.WriteLine("GLSL output is not supported.");
                break;

            // ---- Translate to HLSL
            case Generator.HLSL:
                {
                    Translator hlsl = new();

                    string code = hlsl.Translate(ast);

                    using var stream = new StreamWriter(Path.Join(opts.OutputDirectory, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.hlsl"));
                    stream.Write(code);
                }
                break;

            default:
                Console.Error.WriteLine("Unsupported shader translation output is not supported.");
                break;
        }

        return;
    }

    if (opts.NodeGraph)
    {
        Console.WriteLine("Node Graph compiler is not yet implemented.");
        return;
    }

    Console.Error.WriteLine("You must provide either --shader (to compile a shader file) or --node-graph (to compile a node graph file). Use --help to get help.");
}


Parser parser = new(SetupParser);

parser.ParseArguments<Options>(args)
  .WithParsed(RunOptions);

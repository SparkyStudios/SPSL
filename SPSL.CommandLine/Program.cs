using Antlr4.Runtime;
using SPSL.CommandLine;
using SPSL.Language;
using SPSL.Language.AST;
using SPSL.Language.AST.Visitors;
using CommandLine;

HashSet<string> _importedNamespaces = new();

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
    if (opts.Shader && opts.NodeGraph)
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
        AST ast = ParseFile(opts.InputFile, opts.LibDirectories);

        switch (opts.Generator)
        {
            case Generator.GLSL:
                Console.Error.WriteLine("GLSL output is not supported.");
                break;

            // ---- Translate to HLSL
            case Generator.HLSL:
                {
                    SPSL.Translation.HLSL.Translator hlsl = new();

                    string code = hlsl.Translate(ast);

                    using var stream = new System.IO.StreamWriter(Path.Join(opts.OutputDirectory, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.hlsl"));
                    stream.Write(code);
                }
                break;
        }
    }
}

AST ParseDirectory(string path, IEnumerable<string> libraryPaths)
{
    AST ast = new();

    foreach (var libraryPath in libraryPaths)
    {
        if (!Directory.Exists(libraryPath))
            continue;

        foreach (var file in Directory.GetFiles(Path.Join(libraryPath, path), "*.spsli", SearchOption.AllDirectories))
            ast.Merge(ParseFile(file, libraryPaths));
    }

    return ast;
}

AST ParseFile(string path, IEnumerable<string> libraryPaths)
{
    using var spsl = new System.IO.StreamReader(path);

    // ---- Build AST

    SPSLLexer lexer = new(new AntlrInputStream(spsl));

    lexer.RemoveErrorListeners();

    SPSLParser parser = new(new CommonTokenStream(lexer));
    parser.RemoveErrorListeners();

    ASTVisitor shaderVisitor = new();

    AST ast = shaderVisitor.Visit(parser.file());

    foreach (var import in shaderVisitor.Imports.Where(i => !_importedNamespaces.Contains(i)))
    {
        ast.Merge(ParseDirectory(import, libraryPaths));
        _importedNamespaces.Add(import);
    }

    return ast;
}

CommandLine.Parser parser = new(SetupParser);

parser.ParseArguments<Options>(args)
  .WithParsed(RunOptions);

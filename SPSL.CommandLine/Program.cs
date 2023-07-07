using Antlr4.Runtime;
using CommandLine;
using SPSL.CommandLine;
using SPSL.Language;
using SPSL.Language.AST;
using SPSL.Language.AST.Visitors;
using SPSL.Translation.HLSL;

using Parser = CommandLine.Parser;

HashSet<string> importedNamespaces = new();

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
        AST ast = ParseFile(opts.InputFile, opts.LibDirectories);

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

AST ParseDirectory(string path, IEnumerable<string> libraryPaths)
{
    AST ast = new();
    IEnumerable<string> paths = libraryPaths as string[] ?? libraryPaths.ToArray();

    foreach (var libraryPath in paths.Select(Path.GetFullPath))
    {
        if (!Directory.Exists(libraryPath))
            continue;

        foreach (var file in Directory.GetFiles(Path.Join(libraryPath, path), "*.spsli", SearchOption.AllDirectories))
        {
            var ns = Path.GetDirectoryName(file)![(libraryPath.Length + 1)..].Replace(Path.DirectorySeparatorChar.ToString(), "::");
            var pos = ns.LastIndexOf("::", StringComparison.Ordinal);

            var parsed = ParseFile(file, paths);

            if (pos >= 0)
            {
                var parent = ns[..pos];
                ns = ns[(pos + 1)..];

                if (ast.FirstOrDefault(n => n.FullName == parent) is { } parentNode)
                {
                    parsed[ns].Parent = parentNode;

                    if (parentNode.Namespaces.FirstOrDefault(n => n.FullName == parsed[ns].FullName) is { } parentNamespace)
                        parentNamespace.Merge(parsed[ns]);
                    else
                        parentNode.Children.Add(parsed[ns]);
                }
            }

            importedNamespaces.Add(ns);
            ast.Merge(parsed);
        }
    }

    return ast;
}

AST ParseFile(string path, IEnumerable<string> libraryPaths)
{
    using var spsl = new StreamReader(path);
    IEnumerable<string> paths = libraryPaths as string[] ?? libraryPaths.ToArray();

    // ---- Build AST

    SPSLLexer lexer = new(new AntlrInputStream(spsl));

    lexer.RemoveErrorListeners();

    SPSLParser parser = new(new CommonTokenStream(lexer));
    parser.RemoveErrorListeners();

    ASTVisitor shaderVisitor = new();

    AST ast = shaderVisitor.Visit(parser.file());

    foreach (var import in shaderVisitor.Imports.Where(i => !importedNamespaces.Contains(i)))
    {
        ast.Merge(ParseDirectory(import, paths));
        importedNamespaces.Add(import);
    }

    return ast;
}

Parser parser = new(SetupParser);

parser.ParseArguments<Options>(args)
  .WithParsed(RunOptions);

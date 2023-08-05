using System.Text;
using CommandLine;
using SPSL.CommandLine;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Serialization.Reflection;
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
        var ast = AST.FromShaderFile(opts.InputFile, opts.LibDirectories);

        switch (opts.Generator)
        {
            case Generator.GLSL:
                Console.Error.WriteLine("GLSL output is not supported.");
                break;

            // ---- Translate to HLSL
            case Generator.HLSL:
                {
                    Translator hlsl = new(new());

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

    if (opts.Material)
    {
        // Build AST
        var ast = AST.FromMaterialFile(opts.InputFile, opts.LibDirectories);

        Namespace ns;
        Material material;

        try
        {
            ns = ast.Single(ns => ns.Any(child => child is Material { IsAbstract: false }));
            material = (Material)ns.Single(child => child is Material);
        }
        catch (InvalidOperationException)
        {
            Console.Error.WriteLine("No material or more than one material has been found. Please make sure you compile only one one non-abstract material at a time.");
            Environment.Exit(1);
            return;
        }

        MaterialReflection materialReflection = new(material.Name);

        bool usedMaterialParameters = false;
        List<NamespacedReference> materialShaders = new();
        foreach (var materialShader in material.Children.OfType<MaterialShader>())
        {
            Shader s = new(materialShader.Stage, materialShader.Name)
            {
                ExtendedShader = materialShader.ReferencedShader,
                IsAbstract = false,
                Parent = material.Parent,
            };

            // Imported shader fragments
            s.ImportedShaderFragments.AddRange(materialShader.ImportedShaderFragments);

            // New shader functions
            s.Children.AddRange(materialShader.Children);

            // Material parameters
            if (!usedMaterialParameters)
            {
                s.Children.AddRange(material.Children.OfType<MaterialParameterGroup>().Select(g =>
                {
                    var properties = g.Children.OfType<MaterialParameter>().Where(p => p.Type == MaterialParameterType.Value).Select(p =>
                    {
                        var property = new TypeProperty(p.ValueType, p.Name);
                        property.Annotations.AddRange(p.Annotations);
                        return property;
                    });

                    StructuredBuffer buffer = new(g.Name, properties)
                    {
                        Access = BufferAccess.Constant,
                        Storage = BufferStorage.Undefined,
                    };

                    buffer.Annotations.AddRange(g.Annotations);
                    return buffer;
                }));

                // Material parameters should be used only once in a single material
                usedMaterialParameters = true;
            }

            // Entry points
            var entryPoint = s.Children.SingleOrDefault(child => child is ShaderFunction { IsConstructor: true } || child is IAnnotable a && a.Annotations.Any(a => a.Name == "entry"));
            if (entryPoint is null)
            {
                // Tries in the parent shader
                var shaderNS = ast[materialShader.ReferencedShader.NamespaceName];
                var refShader = shaderNS.GetChild(materialShader.ReferencedShader.NameWithoutNamespace) as Shader;

                if (refShader is null)
                {
                    Console.Error.WriteLine($"Referenced shader '{materialShader.ReferencedShader.Name}' does not exist.");
                    Environment.Exit(1);
                    return;
                }

                entryPoint = refShader.Children.SingleOrDefault(child => child is ShaderFunction { IsConstructor: true } || child is IAnnotable a && a.Annotations.Any(a => a.Name == "entry"));
                if (entryPoint is null)
                {
                    Console.Error.WriteLine($"Unable to find entry point for shader stage {s.Stage}.");
                    Environment.Exit(1);
                    return;
                }
            }

            materialReflection.EntryPoints[s.Stage] = entryPoint.Name;

            ns.AddChild(s);
            materialShaders.Add(s.GetReference());
        }

        List<NamespacedReference> namespaces = new() { ns.GetReference() };
        namespaces.AddRange(material.Children.OfType<MaterialShader>().Select(shader => new NamespacedReference(shader.ReferencedShader.NamespaceName)));

        TranslatorConfiguration config = new()
        {
            Shaders = materialShaders,
            Namespaces = namespaces,
        };

        switch (opts.Generator)
        {
            case Generator.GLSL:
                Console.Error.WriteLine("GLSL output is not supported.");
                Environment.Exit(1);
                return;

            // ---- Translate to HLSL
            case Generator.HLSL:
                {
                    Translator hlsl = new(config);

                    string code = hlsl.Translate(ast);
                    using var stream = new StreamWriter(Path.Join(opts.OutputDirectory, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.hlsl"));
                    stream.Write(code);

                    materialReflection.ShaderByteCode.Data = Encoding.UTF8.GetBytes(code);
                }
                break;

            default:
                Console.Error.WriteLine("Unsupported shader translation output is not supported.");
                Environment.Exit(1);
                return;
        }

        materialReflection.Serialize(Path.Join(opts.OutputDirectory, $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.spslmb"));

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

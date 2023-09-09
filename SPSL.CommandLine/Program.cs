using System.Linq.Expressions;
using System.Text;
using CommandLine;
using SPSL.CommandLine;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Serialization.Reflection;
using SPSL.Translation.HLSL;
using Parser = CommandLine.Parser;

Parser parser = new(SetupParser);

parser.ParseArguments<ShaderOptions, MaterialOptions, PipelineOptions>(args)
    .WithParsed<ShaderOptions>(RunShaderOptions)
    .WithParsed<MaterialOptions>(RunMaterialOptions)
    .WithParsed<PipelineOptions>(RunPipelineOptions);

return 0;

void SetupParser(ParserSettings settings)
{
    settings.AllowMultiInstance = false;
    settings.AutoHelp = true;
    settings.AutoVersion = true;
    settings.CaseSensitive = true;
    settings.HelpWriter = Console.Out;
}

void RunBaseOptions(BaseOptions opts)
{
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
}

void RunShadingOptions(ShadingOptions opts, out Dictionary<string, string> permutations)
{
    permutations = new();

    foreach (PermutationValue permutation in opts.Permutations)
        permutations[permutation.Name] = permutation.Value;
}

void RunShaderOptions(ShaderOptions opts)
{
    RunBaseOptions(opts);
    RunShadingOptions(opts, out var permutations);

    // Build AST
    Ast ast = Ast.FromShaderFile(opts.InputFile, opts.LibDirectories);

    switch (opts.Generator)
    {
        case ShaderSourceGenerator.GLSL:
            Console.Error.WriteLine("GLSL output is not supported.");
            Environment.Exit(1);
            break;

        // ---- Translate to HLSL
        case ShaderSourceGenerator.HLSL:
        {
            Translator hlsl = new(new() { Permutations = permutations });

            string code = hlsl.Translate(ast);

            using var stream = new StreamWriter(Path.Join(opts.OutputDirectory,
                $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.hlsl"));
            stream.Write(code);
        }
            break;

        default:
            Console.Error.WriteLine("Unsupported shader translation output is not supported.");
            break;
    }
}

void RunMaterialOptions(MaterialOptions opts)
{
    RunBaseOptions(opts);
    RunShadingOptions(opts, out var permutations);

    // Build AST
    Ast ast = Ast.FromMaterialFile(opts.InputFile, opts.LibDirectories);

    Namespace ns;
    Material material;

    try
    {
        ns = ast.Single(ns => ns.Any(child => child is Material { IsAbstract: false }));
        material = (Material)ns.Single(child => child is Material);
    }
    catch (InvalidOperationException)
    {
        Console.Error.WriteLine(
            "No material or more than one material has been found. Please make sure you compile only one non-abstract material at a time.");
        Environment.Exit(1);
        return;
    }

    MaterialReflection materialReflection = new(material.Name.Value);

    var usedMaterialParameters = false;
    List<NamespacedReference> materialShaders = new();
    foreach (MaterialShader materialShader in material.Children.OfType<MaterialShader>())
    {
        Shader s = new(materialShader.Stage, materialShader.Name)
        {
            ExtendedShader = materialShader.ReferencedShader,
            IsAbstract = false,
            ParentNamespace = material.ParentNamespace,
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
                var properties = g.Children.OfType<MaterialParameter>()
                    .Where(p => p.Type == MaterialParameterType.Value).Select(p =>
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
        IBlockChild? entryPoint = s.Children.SingleOrDefault(child =>
            child is ShaderFunction { IsConstructor: true } ||
            child is IAnnotated a && a.Annotations.Any(annotation => annotation.IsEntry));
        if (entryPoint is null)
        {
            // Tries in the parent shader
            Namespace shaderNS = ast[materialShader.ReferencedShader.NamespaceName];
            var refShader = shaderNS.GetChild(materialShader.ReferencedShader.NameWithoutNamespace) as Shader;

            if (refShader is null)
            {
                Console.Error.WriteLine($"Referenced shader '{materialShader.ReferencedShader.Name}' does not exist.");
                Environment.Exit(1);
                return;
            }

            entryPoint = refShader.Children.SingleOrDefault(child =>
                child is ShaderFunction { IsConstructor: true } ||
                child is IAnnotated a && a.Annotations.Any(annotation => annotation.IsEntry));
            if (entryPoint is null)
            {
                Console.Error.WriteLine($"Unable to find entry point for shader stage: {s.Stage}.");
                Environment.Exit(1);
                return;
            }
        }

        materialReflection.EntryPoints[s.Stage] = entryPoint.Name.Value;

        ns.AddChild(s);
        materialShaders.Add(s.GetReference());
    }

    List<NamespacedReference> namespaces = new() { ns.GetReference() };
    namespaces.AddRange(material.Children.OfType<MaterialShader>()
        .Select(shader => new NamespacedReference(shader.ReferencedShader.Names)));

    TranslatorConfiguration config = new()
    {
        Shaders = materialShaders,
        Namespaces = namespaces,
        Permutations = permutations
    };

    switch (opts.Generator)
    {
        case ShaderSourceGenerator.GLSL:
            Console.Error.WriteLine("GLSL output is not supported.");
            Environment.Exit(1);
            return;

        // ---- Translate to HLSL
        case ShaderSourceGenerator.HLSL:
        {
            Translator hlsl = new(config);

            string code = hlsl.Translate(ast);
            using var stream = new StreamWriter(Path.Join(opts.OutputDirectory,
                $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.hlsl"));
            stream.Write(code);

            foreach (StreamProperty property in hlsl.ShaderStream.Inputs)
            {
                if (property.Type is BuiltInDataType { Type: BuiltInDataTypeKind.Sampler })
                {
                }
                else
                {
                    InputAttributeDescription description = new()
                    {
                        Format = property.Type switch
                        {
                            PrimitiveDataType type => type.Type switch
                            {
                                PrimitiveDataTypeKind.Boolean => InputAttributeFormat.UInt1,
                                PrimitiveDataTypeKind.Integer => InputAttributeFormat.Int1,
                                PrimitiveDataTypeKind.UnsignedInteger => InputAttributeFormat.UInt1,
                                PrimitiveDataTypeKind.Float => InputAttributeFormat.Float1,
                                _ => throw new NotSupportedException("Invalid type for shader stream input")
                            },
                            BuiltInDataType type => type.Type switch
                            {
                                BuiltInDataTypeKind.Vector2b => InputAttributeFormat.UInt2,
                                BuiltInDataTypeKind.Vector3b => InputAttributeFormat.UInt3,
                                BuiltInDataTypeKind.Vector4b => InputAttributeFormat.UInt4,
                                BuiltInDataTypeKind.Vector2f => InputAttributeFormat.Float2,
                                BuiltInDataTypeKind.Vector3f => InputAttributeFormat.Float3,
                                BuiltInDataTypeKind.Vector4f => InputAttributeFormat.Float4,
                                BuiltInDataTypeKind.Vector2i => InputAttributeFormat.Int2,
                                BuiltInDataTypeKind.Vector3i => InputAttributeFormat.Int3,
                                BuiltInDataTypeKind.Vector4i => InputAttributeFormat.Int4,
                                BuiltInDataTypeKind.Vector2ui => InputAttributeFormat.UInt2,
                                BuiltInDataTypeKind.Vector3ui => InputAttributeFormat.UInt3,
                                BuiltInDataTypeKind.Vector4ui => InputAttributeFormat.UInt4,
                                BuiltInDataTypeKind.Color3 => InputAttributeFormat.Float3,
                                BuiltInDataTypeKind.Color4 => InputAttributeFormat.Float4,
                                _ => throw new NotSupportedException("Invalid type for shader stream input")
                            },
                            _ => throw new NotSupportedException("Invalid type for shader stream input")
                        },
                    };

                    if (property.Annotations.Last(a => a.IsSemantic) is { } annotation)
                    {
                        if (annotation.Identifier.Value == "semantic")
                        {
                            if (annotation.Arguments.ElementAtOrDefault(0) is UserDefinedConstantExpression semantic)
                                description.SemanticName = semantic.Identifier.Name;
                            
                            if (annotation.Arguments.ElementAtOrDefault(1) is ILiteral semanticIndex)
                                description.SemanticIndex = (uint)semanticIndex.Value;
                        }
                        else
                        {
                            description.SemanticName = annotation.Identifier.Value switch
                            {
                                "position" => "POSITION",
                                "texcoord" => "TEXCOORD",
                                "normal" => "NORMAL",
                                "tangent" => "TANGENT",
                                "bitangent" => "BITANGENT",
                                "color" => "COLOR",
                                "boneweights" => "BONEWEIGHTS",
                                "boneindices" => "BONEINDICES",
                                _ => throw new NotSupportedException("Invalid semantic for shader stream input")
                            };

                            if (annotation.Arguments.SingleOrDefault() is ILiteral semanticIndex)
                                description.SemanticIndex = (uint)semanticIndex.Value;
                        }
                    }

                    materialReflection.InputAttributes.Add(description);
                }
            }

            materialReflection.ShaderByteCode.Data = Encoding.UTF8.GetBytes(code);
        }
            break;

        default:
            Console.Error.WriteLine("Unsupported shader translation output is not supported.");
            Environment.Exit(1);
            return;
    }

    materialReflection.Serialize(Path.Join(opts.OutputDirectory,
        $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.spslmb"));
}

void RunPipelineOptions(PipelineOptions opts)
{
    Console.WriteLine("Node Graph compiler is not yet implemented.");
    Environment.Exit(1);
}
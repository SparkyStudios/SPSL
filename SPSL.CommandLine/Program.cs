using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CommandLine;
using SPSL.CommandLine;
using SPSL.CommandLine.Utils;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;
using SPSL.Serialization.Reflection;
using SPSL.Translation.HLSL;
using Parser = CommandLine.Parser;

Parser parser = new(SetupParser);

parser.ParseArguments<ShaderOptions, MaterialOptions, PipelineOptions>(args)
    .WithParsed<ShaderOptions>(RunShaderOptions)
    .WithParsed<MaterialOptions>(RunMaterialOptions)
    .WithParsed<PipelineOptions>(RunPipelineOptions);

return 0;

#region Setup

void SetupParser(ParserSettings settings)
{
    settings.AllowMultiInstance = false;
    settings.AutoHelp = true;
    settings.AutoVersion = true;
    settings.CaseSensitive = true;
    settings.HelpWriter = Console.Out;
}

#endregion

#region CLI Commands

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

    // Scan source code
    SourceCode.Shader(opts.InputFile, opts.LibDirectories, out Ast ast, out SymbolTable symbolTable);

    switch (opts.Generator)
    {
        case ShaderSourceGenerator.GLSL:
            Console.Error.WriteLine("GLSL output is not supported.");
            Environment.Exit(1);
            break;

        // ---- Translate to HLSL
        case ShaderSourceGenerator.HLSL:
        {
            Translator hlsl = new
            (
                new()
                {
                    Shaders = opts.Shaders.Select(name => new NamespacedReference(name)),
                    Permutations = permutations,
                    SymbolTable = symbolTable
                }
            );

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

    // Scan source code
    SourceCode.Material(opts.InputFile, opts.LibDirectories, out Ast ast, out SymbolTable symbolTable);

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

    Dictionary<ShaderStage, string> materialEntryPoints = new();

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
            Namespace? shaderNS = ast.GetNamespace(materialShader.ReferencedShader.NamespaceName);
            if (shaderNS is null)
            {
                Console.Error.WriteLine(
                    $"Unable to find the imported namespace {materialShader.ReferencedShader}. Please make sure to add the path to the library with this namespace using the '-l' option.");
                Environment.Exit(1);
                return;
            }

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

        materialEntryPoints.Add(s.Stage, entryPoint.Name.Value);

        ns.AddChild(s);
        materialShaders.Add(s.GetReference());
    }

    List<NamespacedReference> namespaces = new() { ns.GetReference() };
    namespaces.AddRange(material.Children.OfType<MaterialShader>()
        .Select(shader => new NamespacedReference(shader.ReferencedShader.Names)));


    if (permutations.Any())
    {
        CompileMaterialVariant("custom", permutations, out _);
    }
    else
    {
        var variants = material.Children.OfType<MaterialVariant>().ToArray();
        if (variants.Any())
        {
            foreach (MaterialVariant variant in variants)
            {
                var variantPermutations = new Dictionary<string, string>
                (
                    variant.PermutationValues.Select(value =>
                        new KeyValuePair<string, string>
                        (
                            ((BasicExpression)((BinaryOperationExpression)value).Left).Identifier.Value,
                            DeclarationString.From((IConstantExpression)((BinaryOperationExpression)value).Right)
                        )
                    )
                );

                CompileMaterialVariant(variant.Name.Value, variantPermutations, out _);
            }
        }

        if (opts.GenerateVariants)
        {
            VariantGenerator generator = new(ast, symbolTable);
            JsonObject genVariants = new();

            // Generate the default variant
            CompileMaterialVariant("default", new(), out var usedPermutations);
            genVariants.Add("default", new JsonObject
                (
                    usedPermutations.Select(p =>
                        new KeyValuePair<string, JsonNode?>(p.Key, JsonValue.Create(p.Value))
                    )
                )
            );

            foreach (string permutation in usedPermutations.Keys)
                generator.Add(permutation);

            for (uint i = 0, l = generator.GetVariantCount(); i < l; i++)
            {
                var variantPermutations = new Dictionary<string, string>(generator.GetVariant(i)
                    .Select(p => new KeyValuePair<string, string>(p.Name, p.Value)));
                var hashCode = variantPermutations.GetHashCode().ToString();

                CompileMaterialVariant
                (
                    hashCode,
                    variantPermutations,
                    out _
                );

                genVariants.Add(hashCode, new JsonObject
                    (
                        variantPermutations.Select(p =>
                            new KeyValuePair<string, JsonNode?>(p.Key, JsonValue.Create(p.Value))
                        )
                    )
                );
            }

            using var stream = new FileStream(Path.Join(opts.OutputDirectory,
                $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.variants.json"), FileMode.Create);
            using var writer = new Utf8JsonWriter(stream);

            genVariants.WriteTo(writer);
        }
    }

    return;

    void CompileMaterialVariant
    (
        string variantName,
        Dictionary<string, string> variantPermutations,
        out Dictionary<string, string> usedPermutations
    )
    {
        usedPermutations = new();
        MaterialReflection materialReflection = new(material.Name.Value);

        foreach (var entryPoint in materialEntryPoints)
            materialReflection.EntryPoints.Add(entryPoint.Key, entryPoint.Value);

        TranslatorConfiguration config = new()
        {
            Shaders = materialShaders,
            Namespaces = namespaces,
            Permutations = variantPermutations,
            SymbolTable = symbolTable
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
                    $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.{variantName}.hlsl"));
                stream.Write(code);

                foreach (StreamProperty property in hlsl.ShaderStream.Inputs)
                {
                    InputElementDescription description = new()
                    {
                        Format = property.Type switch
                        {
                            PrimitiveDataType type => type.Type switch
                            {
                                PrimitiveDataTypeKind.Boolean => InputElementFormat.UInt1,
                                PrimitiveDataTypeKind.Integer => InputElementFormat.Int1,
                                PrimitiveDataTypeKind.UnsignedInteger => InputElementFormat.UInt1,
                                PrimitiveDataTypeKind.Float => InputElementFormat.Float1,
                                _ => throw new NotSupportedException("Invalid type for shader stream input")
                            },
                            BuiltInDataType type => type.Type switch
                            {
                                BuiltInDataTypeKind.Vector2b => InputElementFormat.UInt2,
                                BuiltInDataTypeKind.Vector3b => InputElementFormat.UInt3,
                                BuiltInDataTypeKind.Vector4b => InputElementFormat.UInt4,
                                BuiltInDataTypeKind.Vector2f => InputElementFormat.Float2,
                                BuiltInDataTypeKind.Vector3f => InputElementFormat.Float3,
                                BuiltInDataTypeKind.Vector4f => InputElementFormat.Float4,
                                BuiltInDataTypeKind.Vector2i => InputElementFormat.Int2,
                                BuiltInDataTypeKind.Vector3i => InputElementFormat.Int3,
                                BuiltInDataTypeKind.Vector4i => InputElementFormat.Int4,
                                BuiltInDataTypeKind.Vector2ui => InputElementFormat.UInt2,
                                BuiltInDataTypeKind.Vector3ui => InputElementFormat.UInt3,
                                BuiltInDataTypeKind.Vector4ui => InputElementFormat.UInt4,
                                BuiltInDataTypeKind.Color3 => InputElementFormat.Float3,
                                BuiltInDataTypeKind.Color4 => InputElementFormat.Float4,
                                _ => throw new NotSupportedException("Invalid type for shader stream input")
                            },
                            _ => throw new NotSupportedException("Invalid type for shader stream input")
                        },
                    };

                    if (property.Annotations.Last(a => a.IsSemantic) is { } annotation)
                    {
                        if (annotation.Identifier.Value == "semantic")
                        {
                            if (annotation.Arguments
                                    .ElementAtOrDefault(0) is UserDefinedConstantExpression semantic)
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
                                _ => throw new NotSupportedException("Invalid semantic for shader stream input.")
                            };

                            description.SemanticIndex = annotation.Arguments.SingleOrDefault() switch
                            {
                                UnsignedIntegerLiteral semanticIndex => semanticIndex.Value,
                                IntegerLiteral semanticIndex => (uint)semanticIndex.Value,
                                _ => 0
                            };
                        }
                    }

                    if (description.SemanticName.StartsWith("SV_"))
                        continue; // Skip built-in semantics

                    materialReflection.InputElements.Add(description);
                }

                foreach (var permutation in variantPermutations)
                {
                    materialReflection.Permutations.Add(permutation.Key, permutation.Value);
                    usedPermutations.Add(permutation.Key, permutation.Value);
                    hlsl.UsedPermutations.Remove(permutation.Key);
                }

                foreach (DictionaryEntry permutation in hlsl.UsedPermutations)
                {
                    Console.Error.WriteLine(
                        $"A value for the permutation '{permutation.Key}' was not specified in the variant '{variantName}'. The default value was used instead.");
                    materialReflection.Permutations.Add(permutation.Key, permutation.Value);
                    usedPermutations.Add((string)permutation.Key, (string)permutation.Value!);
                }

                foreach (MaterialState state in material.Children.OfType<MaterialState>())
                {
                    if (state.Value is not null)
                    {
                        materialReflection.States.Add(new(state.Name.Value, state.Value));
                    }
                    else
                    {
                        Hashtable components = new();

                        foreach (MaterialStateComponent component in state.Children.OfType<MaterialStateComponent>())
                            components.Add(component.Name.Value, component.Value.Value);

                        materialReflection.States.Add(new(state.Name.Value, components));
                    }
                }

                materialReflection.ShaderByteCode.Data = Encoding.UTF8.GetBytes(code);
                break;
            }

            default:
                Console.Error.WriteLine("Unsupported shader translation output is not supported.");
                Environment.Exit(1);
                return;
        }

        materialReflection.Serialize(Path.Join(opts.OutputDirectory,
            $"{Path.GetFileNameWithoutExtension(opts.InputFile)}.{variantName}.spslb"));
    }
}

void RunPipelineOptions(PipelineOptions opts)
{
    Console.Error.WriteLine("Node Graph compiler is not yet implemented.");
    Environment.Exit(1);
}

#endregion
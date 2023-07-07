using System.Text;
using Antlr4.StringTemplate;
using SPSL.Language;
using SPSL.Language.AST;
using SPSL.Translation.Common;
using Stream = SPSL.Language.AST.Stream;
using Type = SPSL.Language.AST.Type;

namespace SPSL.Translation.HLSL;

public class Translator
{
    private static string TranslateOp(string op)
    {
        return op switch
        {
            "and" => "&&",
            "or" => "||",
            _ => op
        };
    }

    private static string TranslateBuiltInType(BuiltInDataTypeKind type)
    {
        return type switch
        {
            BuiltInDataTypeKind.ArrayCubemap => "TextureCubeArray",
            BuiltInDataTypeKind.ArrayTexture1D => "Texture1DArray",
            BuiltInDataTypeKind.ArrayTexture2D => "Texture2DArray",
            BuiltInDataTypeKind.Color3 => "float3",
            BuiltInDataTypeKind.Color4 => "float4",
            BuiltInDataTypeKind.Cubemap => "TextureCube",
            BuiltInDataTypeKind.Matrix2f => "float2x2",
            BuiltInDataTypeKind.Matrix2x3f => "float2x3",
            BuiltInDataTypeKind.Matrix2x4f => "float2x4",
            BuiltInDataTypeKind.Matrix3f => "float3x3",
            BuiltInDataTypeKind.Matrix3x2f => "float3x2",
            BuiltInDataTypeKind.Matrix3x4f => "float3x4",
            BuiltInDataTypeKind.Matrix4f => "float4x4",
            BuiltInDataTypeKind.Matrix4x2f => "float4x2",
            BuiltInDataTypeKind.Matrix4x3f => "float4x3",
            BuiltInDataTypeKind.Sampler => "SamplerState",
            BuiltInDataTypeKind.Texture1D => "Texture1D",
            BuiltInDataTypeKind.Texture2D => "Texture2D",
            BuiltInDataTypeKind.Texture3D => "Texture3D",
            BuiltInDataTypeKind.Vector2b => "bool2",
            BuiltInDataTypeKind.Vector2f => "float2",
            BuiltInDataTypeKind.Vector2i => "int2",
            BuiltInDataTypeKind.Vector2ui => "uint2",
            BuiltInDataTypeKind.Vector3b => "bool3",
            BuiltInDataTypeKind.Vector3f => "float3",
            BuiltInDataTypeKind.Vector3i => "int3",
            BuiltInDataTypeKind.Vector3ui => "uint3",
            BuiltInDataTypeKind.Vector4b => "bool4",
            BuiltInDataTypeKind.Vector4f => "float4",
            BuiltInDataTypeKind.Vector4i => "int4",
            BuiltInDataTypeKind.Vector4ui => "uint4",
            _ => string.Empty,
        };
    }

    private static string TranslatePrimitiveType(PrimitiveDataTypeKind type)
    {
        return type switch
        {
            PrimitiveDataTypeKind.Boolean => "bool",
            PrimitiveDataTypeKind.Double => "double",
            PrimitiveDataTypeKind.Float => "float",
            PrimitiveDataTypeKind.Integer => "int",
            PrimitiveDataTypeKind.UnsignedInteger => "uint",
            PrimitiveDataTypeKind.Void => "void",
            _ => string.Empty,
        };
    }

    private static INamespaceChild? Resolve(string root, Namespace ns, AST ast)
    {
        if (root == string.Empty)
            return null;

        var pos = root.IndexOf("::");

        if (pos < 0)
        {
            if (ns.GetChild(root) is INamespaceChild namespaceChild)
                return namespaceChild;

            return ast
                .Select(n => n.FullName != ns.FullName && n.GetChild(root) is INamespaceChild c ? c : null)
                .Where(c => c is not null)
                .FirstOrDefault();
        }

        if (ns.FullName != string.Empty && root.StartsWith(ns.FullName))
            return ns.GetChild(root[ns.FullName.Length..].TrimStart(':'));

        foreach (var n in ast.Where(n => n.FullName != ns.FullName))
            if (root.StartsWith(n.FullName))
                return n.GetChild(root[n.FullName.Length..].TrimStart(':'));

        return null;
    }

    private const string SPSL_FINAL_STREAM_POS_KEY = "__SPSL_FINAL_STREAM_OUTPUT__";
    private static readonly HashSet<Tuple<string?, string>> _processedFragments = new();

    private readonly TemplateGroupString _hlslTemplate;
    private readonly TemplateGroupString _baseTemplate;


    private string _currentBase = string.Empty;
    private Shader _currentShader = null!;
    private Shader _currentConcreteShader = null!;
    private Stream _finalStream = new("ShaderStream", Array.Empty<StreamProperty>());

    private string TranslateShaderFragment(NamespacedReference name, Namespace ns, AST ast,
        IDictionary<string, uint>? conflicts, HashSet<IBlockChild>? overriddenFunctions)
    {
        StringBuilder output = new();

        INamespaceChild? child = Resolve(name.Name, ns, ast);
        if (child is ShaderFragment fragment)
        {
            if (!_processedFragments.Contains(new(fragment.Parent?.FullName, fragment.Name)))
            {
                output.AppendLine(Translate(fragment, ns, ast, overriddenFunctions, conflicts));
                _processedFragments.Add(new(fragment.Parent?.FullName, fragment.Name));
            }
        }
        else
        {
            output.AppendLine("// <spsl-error>");
            output.AppendLine(
                $"// Shader fragment not found: {name.Name}. Make sure it exists or his namespace is imported.");
            output.AppendLine("// </spsl-error>");
            output.AppendLine();
        }

        return output.ToString();
    }

    public Translator()
    {
        var hlslTemplate = "SPSL.Translation.Templates.HLSL.stg";
        var baseTemplate = "SPSL.Translation.Templates.Base.stg";

        using (System.IO.Stream? stream = GetType().Assembly.GetManifestResourceStream(hlslTemplate))
        using (StreamReader reader = new(stream!))
            _hlslTemplate = new("HLSL", reader.ReadToEnd());

        using (System.IO.Stream? stream = GetType().Assembly.GetManifestResourceStream(baseTemplate))
        using (StreamReader reader = new(stream!))
            _baseTemplate = new("Base", reader.ReadToEnd());

        _hlslTemplate.ImportTemplates(_baseTemplate);
    }

    public string Translate(AST ast)
    {
        StringBuilder output = new();

        output.Append("""
// <auto-generated>
// Code generated by Sparky Studios (R) SPSL to HLSL Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

""");

        foreach (Namespace ns in ast)
        {
            output.Append(Translate(ns, ast));
        }

        output.Append("""

// End of auto-generated file.
// </auto-generated>
""");

        // Cleanup empty lines
        output.Replace("\r\n", "\n");
        output.Replace("\n\n\n", "\n\n");
        output.Replace("\n\n\n", "\n");
        output.Replace("\n\n\n", "\n\n");

        return output.ToString();
    }

    public string Translate(Namespace ns, AST ast)
    {
        StringBuilder output = new();

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is PermutationVariable))
        {
            output.AppendLine();

            var child = (PermutationVariable)namespaceChild;
            output.Append(Translate(child, ns, ast));
        }

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is GlobalVariable))
        {
            output.AppendLine();

            var child = (GlobalVariable)namespaceChild;
            output.Append(Translate(child, ns, ast));
        }

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is Type { Kind: TypeKind.Enum }))
        {
            output.AppendLine();

            var child = (Type)namespaceChild;
            output.Append(Translate(child, ns, ast));
        }

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is Type { Kind: TypeKind.Struct }))
        {
            output.AppendLine();

            var child = (Type)namespaceChild;
            output.Append(Translate(child, ns, ast));
        }

        if (ns.Where(child => child is Shader { IsAbstract: false }).Any())
        {
            output.AppendLine();
            output.AppendLine(SPSL_FINAL_STREAM_POS_KEY);
        }

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is Shader { IsAbstract: false }))
        {
            var child = (Shader)namespaceChild;
            _currentConcreteShader = child;
            output.Append(Translate(child, ns, ast));
        }

        output.Replace(SPSL_FINAL_STREAM_POS_KEY, Translate(_finalStream, ns, ast));

        return output.ToString();
    }

    public string Translate(PermutationVariable permutationVariable, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("permutation_var");
        template.Add("m", new Macro(permutationVariable.Name, Translate(permutationVariable.Initializer, ns, ast)));

        if (permutationVariable.Type == PermutationVariable.VariableType.Enum)
        {
            for (int i = 0; i < permutationVariable.EnumerationValues.Length; i++)
            {
                string item = permutationVariable.EnumerationValues[i];
                template.Add("enum_values", new Macro(item, i.ToString()));
            }
        }

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(GlobalVariable global, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        // TODO: Handle name conflicts between namespaces

        Template template = _hlslTemplate.GetInstanceOf("const_global_var");
        template.Add("isStatic", global.IsStatic);
        template.Add("type", Translate(global.Type, ns, ast));
        template.Add("name", global.Name);
        template.Add("initializer", Translate(global.Initializer, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(Type type, Namespace ns, AST ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        switch (type.Kind)
        {
            case TypeKind.Struct:
                {
                    Template template = _hlslTemplate.GetInstanceOf("struct");
                    template.Add("name", type.Name);
                    foreach (TypeProperty property in type.Properties)
                        template.Add("properties", new Prop(Translate(property.Type, ns, ast), property.Name));
                    foreach (TypeFunction function in type.Functions)
                        template.Add("functions", Translate(function.Function, ns, ast));

                    output.Append(template.Render());
                    break;
                }
            case TypeKind.Enum:
                {
                    Template template = _hlslTemplate.GetInstanceOf("enum");
                    template.Add("name", type.Name);
                    foreach (TypeProperty member in type.Properties)
                    {
                        template.Add
                        (
                            "properties",
                            new Prop
                            (
                                Translate(member.Type, ns, ast),
                                member.Name,
                                member.Initializer != null ? Translate(member.Initializer, ns, ast) : null
                            )
                        );
                    }

                    output.Append(template.Render());
                    break;
                }
        }

        return output.ToString();
    }

    public string Translate(ShaderFragment fragment, Namespace ns, AST ast,
        IEnumerable<IBlockChild>? shouldOverride = null, IDictionary<string, uint>? conflicts = null)
    {
        StringBuilder output = new();
        HashSet<IBlockChild> overriddenChildren = new();

        foreach (IBlockChild child in fragment.Children)
            if ((child is ShaderFunction function && function.IsOverride) || child is GlobalVariable)
                overriddenChildren.Add(child);

        if (fragment.ExtendedShaderFragment != NamespacedReference.Null)
            output.Append(TranslateShaderFragment(fragment.ExtendedShaderFragment, ns, ast, conflicts,
                overriddenChildren));

        if (fragment.ImportedShaderFragments.Count > 0)
        {
            Dictionary<string, uint> fragmentChildren = new();

            foreach (NamespacedReference import in fragment.ImportedShaderFragments)
            {
                if (import == NamespacedReference.Null)
                    continue;

                INamespaceChild? frag = Resolve(import.Name, ns, ast);
                if (frag is ShaderFragment shaderFragment)
                {
                    foreach (IBlockChild child in shaderFragment.Children)
                    {
                        string name = child.Name;

                        if (child is ShaderFunction function)
                        {
                            name = Translate(function.Function.Head, ns, ast);
                        }

                        if (fragmentChildren.ContainsKey(name))
                            fragmentChildren[name] += 1;
                        else
                            fragmentChildren.Add(name, 1);
                    }
                }
            }

            foreach (NamespacedReference import in fragment.ImportedShaderFragments)
            {
                if (import == NamespacedReference.Null)
                    continue;

                output.Append(TranslateShaderFragment(import, ns, ast, fragmentChildren, null));
            }
        }

        foreach (Stream stream in fragment.Streams)
            foreach (StreamProperty property in stream.Properties)
                _finalStream.Properties.Add(property);

        _currentBase = Translate(fragment.ExtendedShaderFragment, ns, ast);

        foreach (PermutationVariable permutation in fragment.PermutationVariables)
        {
            output.AppendLine();

            var name = permutation.Name;
            if (shouldOverride?.SingleOrDefault(m =>
                    m is PermutationVariable permutationVariable && permutationVariable.Name.Equals(permutation.Name)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                permutation.Name = $"{fragment.Name}_{permutation.Name}";

            output.Append(Translate(permutation, ns, ast));
        }

        foreach (GlobalVariable variable in fragment.GlobalVariables)
        {
            output.AppendLine();

            var name = variable.Name;
            if (shouldOverride?.SingleOrDefault(m =>
                    m is GlobalVariable globalVariable && globalVariable.Name.Equals(variable.Name)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                variable.Name = $"{fragment.Name}_{variable.Name}";

            output.Append(Translate(variable, ns, ast));
        }

        foreach (Type type in fragment.Types)
        {
            output.AppendLine();

            var name = type.Name;
            if (shouldOverride?.SingleOrDefault(m =>
                    m is Type shaderType &&
                    shaderType.Name.Equals(type.Name)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                type.Name = $"{fragment.Name}_{type.Name}";

            output.Append(Translate(type, ns, ast));
        }

        foreach (StructuredBuffer buffer in fragment.StructuredBuffers)
        {
            output.AppendLine();

            var name = buffer.Name;
            if (shouldOverride?.SingleOrDefault(m =>
                    m is StructuredBuffer shaderBuffer &&
                    shaderBuffer.Name.Equals(buffer.Name)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                buffer.Name = $"{fragment.Name}_{buffer.Name}";

            output.Append(Translate(buffer, ns, ast));
        }

        foreach (TypedBuffer buffer in fragment.TypedBuffers)
        {
            output.AppendLine();

            var name = buffer.Name;
            if (shouldOverride?.SingleOrDefault(m =>
                    m is TypedBuffer shaderBuffer &&
                    shaderBuffer.Name.Equals(buffer.Name)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                buffer.Name = $"{fragment.Name}_{buffer.Name}";

            output.Append(Translate(buffer, ns, ast));
        }

        foreach (ShaderFunction function in fragment.Functions)
        {
            output.AppendLine();

            var name = Translate(function.Function.Head, ns, ast);
            if (shouldOverride?.SingleOrDefault(m =>
                    m is ShaderFunction shaderFunction &&
                    shaderFunction.Function.Head.Equals(function.Function.Head)) != null ||
                (conflicts is not null && conflicts.ContainsKey(name) && conflicts[name] > 1))
                function.Function.Head.Name = $"{fragment.Name}_{function.Function.Head.Name}";

            output.Append(Translate(function, ns, ast));
        }

        return output.ToString();
    }

    public string Translate(Shader shader, Namespace ns, AST ast, IEnumerable<IBlockChild>? shouldOverride = null)
    {
        _currentShader = shader;

        // 1. Process base shaders recursively
        //    - Rename overridden methods with BaseShader_MethodName
        // 2. Process shader fragments recursively
        //    - Latest shader fragment functions got predominance on previous ones if they have the same signature.
        // 3. Process this shader
        //    - Calls to this are removed
        //    - Calls to base are renamed with the parent shader name

        StringBuilder output = new();
        HashSet<IBlockChild> overriddenChildren = new();

        foreach (IBlockChild child in shader.Children)
            if (child is ShaderFunction function && function.IsOverride)
                overriddenChildren.Add(function);

        if (shader.ExtendedShader != NamespacedReference.Null)
        {
            INamespaceChild? parent = Resolve(shader.ExtendedShader.Name, ns, ast);
            if (parent is Shader parentShader)
            {
                output.Append(Translate(parentShader, ns, ast, overriddenChildren));

                // Set back the current shader
                _currentShader = shader;
            }
            else
            {
                output.AppendLine("// <spsl-error>");
                output.AppendLine(
                    $"// Shader not found: {shader.ExtendedShader.Name}. Make sure it exists or his namespace is imported.");
                output.AppendLine("// </spsl-error>");
                output.AppendLine();
            }
        }

        if (shader.ImportedShaderFragments.Count > 0)
        {
            Dictionary<string, uint> fragmentChildren = new();

            foreach (NamespacedReference fragment in shader.ImportedShaderFragments)
            {
                if (fragment == NamespacedReference.Null)
                    continue;

                INamespaceChild? frag = Resolve(fragment.Name, ns, ast);
                if (frag is not ShaderFragment shaderFragment) continue;

                foreach (IBlockChild child in shaderFragment.Children)
                {
                    var name = child.Name;

                    if (child is ShaderFunction function)
                    {
                        name = Translate(function.Function.Head, ns, ast);
                    }

                    if (fragmentChildren.ContainsKey(name))
                        fragmentChildren[name] += 1;
                    else
                        fragmentChildren.Add(name, 1);
                }
            }

            foreach (NamespacedReference import in shader.ImportedShaderFragments)
            {
                if (import == NamespacedReference.Null)
                    continue;

                output.Append(TranslateShaderFragment(import, ns, ast, fragmentChildren, null));
            }
        }

        _currentBase = Translate(shader.ExtendedShader, ns, ast);

        output.AppendLine();

        // Streams
        foreach (IBlockChild child in shader.Children)
            if (child is Stream stream)
                foreach (StreamProperty property in stream.Properties)
                    _finalStream.Properties.Add(property);

        // Functions declaration
        foreach (IBlockChild child in shader.Children)
        {
            if (child is not ShaderFunction shaderFunction)
                continue;

            if (shouldOverride?.SingleOrDefault(m =>
                    m is ShaderFunction sf && sf.Function.Head.Equals(shaderFunction.Function.Head)) != null)
                shaderFunction.Function.Head.Name = $"{shader.Name}_{shaderFunction.Function.Head.Name}";

            Annotation? entry = shaderFunction.Annotations.SingleOrDefault(a => a.Name == "entry");

            if (entry != null || shaderFunction.Name == _currentShader.Name)
            {
                if (_currentShader.Type == ShaderType.Compute)
                {
                    // -- Compute shader entry points need the numthreads attribute

                    output.AppendLine();
                    output.AppendFormat("[[numthreads({0}, {1}, {2})]]", _currentShader.ComputeParams.ThreadCountX, _currentShader.ComputeParams.ThreadCountY, _currentShader.ComputeParams.ThreadCountZ);

                    // -- Ensure that we have all our compute shader keywords as parameters

                    if (shaderFunction.Function.Head.Signature.Parameters.Where(p => p.Name == "sp_GroupThreadId").Any() == false)
                    {
                        shaderFunction.Function.Head.Signature.Parameters.Add
                        (
                            new FunctionArgument
                            (
                                DataFlow.Unspecified,
                                new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                "sp_GroupThreadId : SV_GroupThreadID"
                            )
                        );
                    }

                    if (shaderFunction.Function.Head.Signature.Parameters.Where(p => p.Name == "sp_GroupId").Any() == false)
                    {
                        shaderFunction.Function.Head.Signature.Parameters.Add
                        (
                            new FunctionArgument
                            (
                                DataFlow.Unspecified,
                                new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                "sp_GroupId : SV_GroupID"
                            )
                        );
                    }

                    if (shaderFunction.Function.Head.Signature.Parameters.Where(p => p.Name == "sp_DispatchThreadId").Any() == false)
                    {
                        shaderFunction.Function.Head.Signature.Parameters.Add
                        (
                            new FunctionArgument
                            (
                                DataFlow.Unspecified,
                                new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                "sp_DispatchThreadId : SV_DispatchThreadID"
                            )
                        );
                    }

                    if (shaderFunction.Function.Head.Signature.Parameters.Where(p => p.Name == "sp_GroupIndex").Any() == false)
                    {
                        shaderFunction.Function.Head.Signature.Parameters.Add
                        (
                            new FunctionArgument
                            (
                                DataFlow.Unspecified,
                                new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger),
                                "sp_GroupIndex : SV_GroupIndex"
                            )
                        );
                    }
                }
            }

            if ((entry != null && _currentShader.Type == ShaderType.Graphic && entry.Arguments.Where(a => Translate(a, ns, ast) == "vertex").Any()) || (_currentShader.Type == ShaderType.Vertex && shaderFunction.Name == _currentShader.Name))
            {
                // Vertex shader entry point always returns a ShaderStream_Transient struct
                shaderFunction.Function.Head.ReturnType = new UserDefinedDataType(new("ShaderStream_Transient"));

                // Vertex shader entry point always takes as arguments a single ShaderStream_Input struct
                shaderFunction.Function.Head.Signature.Parameters.Clear();
                shaderFunction.Function.Head.Signature.Parameters.Add(new FunctionArgument(DataFlow.In, new UserDefinedDataType(new("ShaderStream_Input")), "streams"));
            }

            output.Append(Translate(shaderFunction.Function.Head, ns, ast));
            output.AppendLine(";");
        }

        // Shader members & functions definition
        foreach (IBlockChild child in shader.Children)
        {
            if (child is ShaderFunction shaderFunction)
                if (shouldOverride?.SingleOrDefault(m =>
                        m is ShaderFunction sf && sf.Function.Head.Equals(shaderFunction.Function.Head)) != null)
                    shaderFunction.Function.Head.Name = $"{shader.Name}_{shaderFunction.Function.Head.Name}";

            var code = child switch
            {
                IShaderMember member => Translate(member, ns, ast),
                Function function => Translate(function, ns, ast),
                ShaderFunction sf => Translate(sf, ns, ast),
                _ => string.Empty,
            };

            output.AppendLine(code);
        }

        return output.ToString();
    }

    public string Translate(IShaderMember member, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = member switch
        {
            StructuredBuffer buffer => Translate(buffer, ns, ast),
            TypedBuffer buffer => Translate(buffer, ns, ast),
            Type type => Translate(type, ns, ast),
            _ => string.Empty,
        };

        output.Append(code);

        return output.ToString();
    }

    public string Translate(StructuredBuffer buffer, Namespace ns, AST ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        Template? template = null;

        if (buffer.Access == BufferAccess.Constant)
        {
            template = _hlslTemplate.GetInstanceOf("cbuffer");
        }
        else if (buffer.Access == BufferAccess.WriteOnly || buffer.Access == BufferAccess.ReadWrite)
        {
            template = _hlslTemplate.GetInstanceOf("rwsbuffer");
            template.Add("isCoherent", (buffer.Storage & BufferStorage.Coherent) == BufferStorage.Coherent);
        }
        else
        {
            template = _hlslTemplate.GetInstanceOf("sbuffer");
        }

        if (template == null)
        {
            output.Clear();

            output.AppendLine("// <spsl-error>");
            output.AppendLine("// Buffer declaration not supported by HLSL translator.");
            output.AppendLine("// </spsl-error>");
            output.AppendLine();

            return output.ToString();
        }

        template.Add("name", buffer.Name);

        string? binding = null;
        string? set = null;

        foreach (Annotation annotation in buffer.Annotations)
        {
            if (annotation.Name != "register")
                continue;

            if (annotation.Arguments.Count != 2)
            {
                output.Clear();

                output.AppendLine("// <spsl-error>");
                output.AppendLine("// Invalid usage of the @register(<binding>, <set>) annotation.");
                output.AppendLine("// </spsl-error>");
                output.AppendLine();

                return output.ToString();
            }

            binding = Translate(annotation.Arguments[0], ns, ast);
            set = Translate(annotation.Arguments[1], ns, ast);
        }

        template.Add("binding", binding);
        template.Add("set", set);

        foreach (TypeProperty property in buffer.Properties)
            template.Add("properties", new Prop(Translate(property.Type, ns, ast), property.Name));

        output.Append(template.Render());

        return output.ToString();
    }

    public string Translate(TypedBuffer buffer, Namespace ns, AST ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        Template? template = null;

        bool isStructured = buffer.DataType switch
        {
            PrimitiveDataType => false,
            UserDefinedDataType => true,
            BuiltInDataType t => t.Type switch
            {
                BuiltInDataTypeKind.Color3 => true,
                BuiltInDataTypeKind.Color4 => true,
                BuiltInDataTypeKind.Vector2b => true,
                BuiltInDataTypeKind.Vector2f => true,
                BuiltInDataTypeKind.Vector2i => true,
                BuiltInDataTypeKind.Vector2ui => true,
                BuiltInDataTypeKind.Vector3b => true,
                BuiltInDataTypeKind.Vector3f => true,
                BuiltInDataTypeKind.Vector3i => true,
                BuiltInDataTypeKind.Vector3ui => true,
                BuiltInDataTypeKind.Vector4b => true,
                BuiltInDataTypeKind.Vector4f => true,
                BuiltInDataTypeKind.Vector4i => true,
                BuiltInDataTypeKind.Vector4ui => true,
                _ => false,
            },
            _ => false,
        };

        if (buffer.Access == BufferAccess.Constant)
        {
            output.Clear();

            output.AppendLine("// <spsl-error>");
            output.AppendLine("// An SPSL TypedBuffer cannot be constant. Failed buffer name: " + buffer.Name);
            output.AppendLine("// </spsl-error>");
            output.AppendLine();

            return output.ToString();
        }
        else if (buffer.Access == BufferAccess.WriteOnly || buffer.Access == BufferAccess.ReadWrite)
        {
            template = _hlslTemplate.GetInstanceOf("rwtbuffer");
            template.Add("isCoherent", (buffer.Storage & BufferStorage.Coherent) == BufferStorage.Coherent);
            template.Add("isStructured", isStructured);
        }
        else
        {
            template = _hlslTemplate.GetInstanceOf("tbuffer");
            template.Add("isStructured", isStructured);
        }

        if (template == null)
        {
            output.Clear();

            output.AppendLine("// <spsl-error>");
            output.AppendLine("// Buffer declaration not supported by HLSL translator.");
            output.AppendLine("// </spsl-error>");
            output.AppendLine();

            return output.ToString();
        }

        template.Add("name", buffer.Name);
        template.Add("type", Translate(buffer.DataType, ns, ast));

        string? binding = null;
        string? set = null;

        foreach (Annotation annotation in buffer.Annotations)
        {
            if (annotation.Name != "register")
                continue;

            if (annotation.Arguments.Count != 2)
            {
                output.Clear();

                output.AppendLine("// <spsl-error>");
                output.AppendLine("// Invalid usage of the @register(<binding>, <set>) annotation.");
                output.AppendLine("// </spsl-error>");
                output.AppendLine();

                return output.ToString();
            }

            binding = Translate(annotation.Arguments[0], ns, ast);
            set = Translate(annotation.Arguments[1], ns, ast);
        }

        template.Add("binding", binding);
        template.Add("set", set);

        output.Append(template.Render());

        return output.ToString();
    }

    public string Translate(Stream stream, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Action<StreamProperty, Template> TranslateProperty = (property, template) =>
        {
            var annotation = property.Annotations.Where(a => a.Name == "semantic").SingleOrDefault();
            var expression = annotation?.Arguments.SingleOrDefault();
            string? semantic = expression is not null ? Translate(expression, ns, ast) : null;
            template.Add("properties", new Prop(Translate(property.Type, ns, ast), property.Name, null, semantic));
        };

        // Vertex Shader Input
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", $"{stream.Name}_Input");
            foreach (StreamProperty property in stream.Inputs)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        // Transient Shader Data / Pixel Shader Input
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", $"{stream.Name}_Transient");
            foreach (StreamProperty property in stream.Transients)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        // Pixel Shader Output
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", $"{stream.Name}_Output");
            foreach (StreamProperty property in stream.Outputs)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        return output.ToString();
    }

    public string Translate(ShaderFunction function, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(Translate(function.Function, ns, ast));

        return output.ToString();
    }

    public string Translate(Function function, Namespace ns, AST ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        output.AppendLine(Translate(function.Head, ns, ast));
        output.Append(Translate(function.Body, ns, ast));

        return output.ToString();
    }

    public string Translate(FunctionHead head, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("func_head");
        template.Add("type", Translate(head.ReturnType, ns, ast));
        template.Add("name", head.Name);
        foreach (FunctionArgument arg in head.Signature.Parameters)
        {
            template.Add("args", new Arg(arg.Flow switch
            {
                DataFlow.In => "in",
                DataFlow.InOut => "inout",
                DataFlow.Out => "out",
                DataFlow.Const => "const",
                _ => null
            }, Translate(arg.Type, ns, ast), arg.Name));
        }

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(IDataType dataType, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = dataType switch
        {
            BuiltInDataType builtInType => Translate(builtInType, ns, ast),
            PrimitiveDataType primitiveType => Translate(primitiveType, ns, ast),
            UnknownDataType unknownType => Translate(unknownType, ns, ast),
            UserDefinedDataType userDefinedType => Translate(userDefinedType, ns, ast),
            _ => string.Empty,
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(BuiltInDataType builtInType, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(TranslateBuiltInType(builtInType.Type));

        return output.ToString();
    }

    public string Translate(PrimitiveDataType primitiveType, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(TranslatePrimitiveType(primitiveType.Type));

        return output.ToString();
    }

    public string Translate(UnknownDataType unknownType, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("__unknown_type__");

        return output.ToString();
    }

    public string Translate(UserDefinedDataType userDefinedType, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(Translate(userDefinedType.Type, ns, ast));

        return output.ToString();
    }

    public string Translate(IExpression expression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = expression switch
        {
            IConstantExpression constantExpression => Translate(constantExpression, ns, ast),
            ArrayAccessExpression arrayAccessExpression => Translate(arrayAccessExpression, ns, ast),
            AssignmentExpression assignmentExpression => Translate(assignmentExpression, ns, ast),
            BasicExpression basicExpression => Translate(basicExpression, ns, ast),
            BinaryOperationExpression binaryExpression => Translate(binaryExpression, ns, ast),
            CastExpression castExpression => Translate(castExpression, ns, ast),
            InvocationExpression invocationExpression => Translate(invocationExpression, ns, ast),
            MethodMemberReferenceExpression methodMemberReferenceExpression => Translate(
                methodMemberReferenceExpression, ns, ast),
            NegateOperationExpression negateOperationExpression => Translate(negateOperationExpression, ns, ast),
            NewInstanceExpression newInstanceExpression => Translate(newInstanceExpression, ns, ast),
            ParenthesizedExpression parenthesizedExpression => Translate(parenthesizedExpression, ns, ast),
            PropertyMemberReferenceExpression propertyMemberReferenceExpression => Translate(
                propertyMemberReferenceExpression, ns, ast),
            TernaryOperationExpression ternaryOperationExpression => Translate(ternaryOperationExpression, ns, ast),
            UnaryOperationExpression unaryExpression => Translate(unaryExpression, ns, ast),
            SignedExpression signedExpression => Translate(signedExpression, ns, ast),
            ChainedExpression chainedMemberReferenceExpression => Translate(chainedMemberReferenceExpression, ns, ast),
            _ => string.Empty
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(IConstantExpression constantExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = constantExpression switch
        {
            IPrimitiveExpression primitiveExpression => Translate(primitiveExpression, ns, ast),
            UserDefinedConstantExpression userDefinedConstantExpression => Translate(userDefinedConstantExpression, ns,
                ast),
            _ => string.Empty,
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(IPrimitiveExpression primitiveExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = primitiveExpression switch
        {
            ILiteral literal => Translate(literal, ns, ast),
            _ => string.Empty
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(ILiteral literal, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var code = literal switch
        {
            BoolLiteral boolLiteral => Translate(boolLiteral, ns, ast),
            DoubleLiteral doubleLiteral => Translate(doubleLiteral, ns, ast),
            FloatLiteral floatLiteral => Translate(floatLiteral, ns, ast),
            IntegerLiteral integerLiteral => Translate(integerLiteral, ns, ast),
            StringLiteral stringLiteral => Translate(stringLiteral, ns, ast),
            UnsignedIntegerLiteral unsignedIntegerLiteral => Translate(unsignedIntegerLiteral, ns, ast),
            _ => string.Empty
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(BoolLiteral boolLiteral, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(boolLiteral.Value.ToString().ToLower());

        return output.ToString();
    }

    public string Translate(DoubleLiteral doubleLiteral, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(doubleLiteral.Value);

        return output.ToString();
    }

    public string Translate(FloatLiteral floatLiteral, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(floatLiteral.Value);

        if (Math.Abs((int)floatLiteral.Value - floatLiteral.Value) == 0)
            output.Append(".0");

        output.Append('f');

        return output.ToString();
    }

    public string Translate(IntegerLiteral integerLiteral, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        if (integerLiteral.IsHexConstant)
            output.Append("0x");

        if (integerLiteral.IsOctalConstant)
            output.Append('0');

        output.Append(Convert.ToString(integerLiteral.Value,
            integerLiteral.IsHexConstant ? 16 : integerLiteral.IsOctalConstant ? 8 : 10));

        return output.ToString();
    }

    public string Translate(StringLiteral literal, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(literal.Value);

        return output.ToString();
    }

    public string Translate(UnsignedIntegerLiteral unsignedIntegerLiteral, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(unsignedIntegerLiteral.Value);

        return output.ToString();
    }

    public string Translate(UserDefinedConstantExpression userDefinedConstantExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(Translate(userDefinedConstantExpression.Identifier, ns, ast));

        return output.ToString();
    }

    public string Translate(ArrayAccessExpression arrayAccessExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("array_access");
        template.Add("var", Translate(arrayAccessExpression.Expression, ns, ast));
        template.Add("idx", Translate(arrayAccessExpression.ArrayIndex, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(AssignmentExpression assignmentExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("binary_op");
        template.Add("lhs", Translate(assignmentExpression.Left, ns, ast));
        template.Add("op", TranslateOp(assignmentExpression.Operator));
        template.Add("rhs", Translate(assignmentExpression.Right, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(BasicExpression basicExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(basicExpression.Identifier);

        return output.ToString();
    }

    public string Translate(BinaryOperationExpression binaryExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template;

        // Use pow function instead
        if (binaryExpression.Operator == "^")
        {
            template = _hlslTemplate.GetInstanceOf("invocation");
            template.Add("function", "pow");
            template.Add("params", Translate(binaryExpression.Left, ns, ast));
            template.Add("params", Translate(binaryExpression.Right, ns, ast));
        }
        else
        {
            template = _hlslTemplate.GetInstanceOf("binary_op");
            template.Add("lhs", Translate(binaryExpression.Left, ns, ast));
            template.Add("op", TranslateOp(binaryExpression.Operator));
            template.Add("rhs", Translate(binaryExpression.Right, ns, ast));
        }

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(CastExpression castExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("cast");
        template.Add("type", Translate(castExpression.Type, ns, ast));
        template.Add("value", Translate(castExpression.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(InvocationExpression invocationExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("invocation");
        template.Add("function", Translate(invocationExpression.Name, ns, ast));

        foreach (InvocationParameter parameter in invocationExpression.Parameters)
            template.Add("params", Translate(parameter.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(ChainedExpression chainedMemberReferenceExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(Translate(chainedMemberReferenceExpression.Target, ns, ast));
        output.Append('.');
        output.Append
        (
            string.Join
            (
                '.',
                chainedMemberReferenceExpression.Members.Select(expression => Translate(expression, ns, ast))
            )
        );

        return output.ToString();
    }

    public string Translate(MethodMemberReferenceExpression methodMemberReferenceExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template;

        switch (methodMemberReferenceExpression.Target)
        {
            // Calls to this are ignored
            case "this":
                template = _hlslTemplate.GetInstanceOf("invocation");
                template.Add("function", methodMemberReferenceExpression.Member.Name);
                break;

            // Calls to base are renamed
            case "base":
                template = _hlslTemplate.GetInstanceOf("invocation");
                template.Add("function", $"{_currentBase}_{methodMemberReferenceExpression.Member.Name}");
                break;

            default:
                template = _hlslTemplate.GetInstanceOf("member_invocation");
                template.Add("owner", methodMemberReferenceExpression.Target);
                template.Add("function", methodMemberReferenceExpression.Member.Name);
                break;
        }

        foreach (InvocationParameter parameter in methodMemberReferenceExpression.Member.Parameters)
            template.Add("params", Translate(parameter.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(NegateOperationExpression negateOperationExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append('!');
        output.Append(Translate(negateOperationExpression.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(NewInstanceExpression newInstanceExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("new_instance");
        template.Add("type", Translate(newInstanceExpression.Type, ns, ast));

        foreach (InvocationParameter argument in newInstanceExpression.Parameters)
            template.Add("params", Translate(argument.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(ParenthesizedExpression parenthesizedExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append('(');
        output.Append(Translate(parenthesizedExpression.Expression, ns, ast));
        output.Append(')');

        return output.ToString();
    }

    public string Translate(PropertyMemberReferenceExpression propertyMemberReferenceExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        switch (propertyMemberReferenceExpression.Target)
        {
            // Calls to this are ignored
            case "this":
                output.Append(propertyMemberReferenceExpression.Member);
                break;

            // Calls to base are renamed
            case "base":
                output.Append($"{_currentBase}_{propertyMemberReferenceExpression.Member}");
                break;

            default:
                output.Append($"{propertyMemberReferenceExpression.Target}.{propertyMemberReferenceExpression.Member}");
                break;
        }

        return output.ToString();
    }

    public string Translate(TernaryOperationExpression ternaryOperationExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("ternary_op");
        template.Add("condition", Translate(ternaryOperationExpression.Condition, ns, ast));
        template.Add("lhs", Translate(ternaryOperationExpression.WhenTrue, ns, ast));
        template.Add("rhs", Translate(ternaryOperationExpression.WhenFalse, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(UnaryOperationExpression unaryExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("unary_op");
        template.Add("op", TranslateOp(unaryExpression.Operator));
        template.Add("expression", Translate(unaryExpression.Expression, ns, ast));
        template.Add("isPostfix", unaryExpression.IsPostfix);

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(SignedExpression signedExpression, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(signedExpression.Sign);
        output.Append(Translate(signedExpression.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(IStatement statement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var needComma = true;

        switch (statement)
        {
            case BreakStatement breakStatement:
                output.Append(Translate(breakStatement, ns, ast));
                break;
            case ContinueStatement continueStatement:
                output.Append(Translate(continueStatement, ns, ast));
                break;
            case DiscardStatement discardStatement:
                output.Append(Translate(discardStatement, ns, ast));
                break;
            case ExpressionStatement expressionStatement:
                output.Append(Translate(expressionStatement, ns, ast));
                break;
            case IfStatement ifStatement:
                output.Append(Translate(ifStatement, ns, ast));
                needComma = false;
                break;
            case WhileStatement whileStatement:
                output.Append(Translate(whileStatement, ns, ast));
                needComma = false;
                break;
            case ReturnStatement ret:
                output.Append(Translate(ret, ns, ast));
                break;
            case StatementBlock block:
                output.Append(Translate(block, ns, ast));
                needComma = false;
                break;
            case StatementCollection statementCollection:
                output.Append(Translate(statementCollection, ns, ast));
                needComma = false;
                break;
            case VariableDeclarationStatement variableDeclarationStatement:
                output.Append(Translate(variableDeclarationStatement, ns, ast));
                break;
            case PermuteStatement permuteStatement:
                output.Append(Translate(permuteStatement, ns, ast));
                needComma = false;
                break;
        }

        if (needComma)
            output.Append(';');

        return output.ToString();
    }

    public string Translate(BreakStatement breakStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("break");

        return output.ToString();
    }

    public string Translate(ContinueStatement continueStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("continue");

        return output.ToString();
    }

    public string Translate(DiscardStatement discardStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("discard");

        return output.ToString();
    }

    public string Translate(ExpressionStatement expressionStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append(Translate(expressionStatement.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(IfStatement ifStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("if (");
        output.Append(Translate(ifStatement.If.Condition, ns, ast));
        output.AppendLine(")");
        output.Append(Translate(ifStatement.If.Block, ns, ast));

        foreach (IfStatement.IfStatementConditionBlock elif in ifStatement.Elif)
        {
            output.Append("else if (");
            output.Append(Translate(elif.Condition, ns, ast));
            output.AppendLine(")");
            output.Append(Translate(elif.Block, ns, ast));
        }

        if (ifStatement.Else != null)
        {
            output.AppendLine("else");
            output.Append(Translate(ifStatement.Else, ns, ast));
        }

        return output.ToString();
    }

    public string Translate(WhileStatement whileStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("while (");
        output.Append(Translate(whileStatement.Condition, ns, ast));
        output.AppendLine(")");
        output.Append(Translate(whileStatement.Block, ns, ast));

        return output.ToString();
    }

    public string Translate(ReturnStatement ret, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        output.Append("return");

        if (ret.Expression != null)
        {
            output.Append(' ');
            output.Append(Translate(ret.Expression, ns, ast));
        }

        return output.ToString();
    }

    public string Translate(StatementBlock body, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("statements_block");
        foreach (IStatement statement in body.Children)
            template.Add("stats", Translate(statement, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(StatementCollection statementCollection, Namespace ns, AST ast)
    {
        StringBuilder output = new();
        IEnumerable<string> statements =
            statementCollection.Statements.Select(statement => Translate(statement, ns, ast));

        output.Append(string.Join('\n', statements));
        return output.ToString();
    }

    public string Translate(VariableDeclarationStatement variableDeclarationStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("variable_declaration");
        template.Add("isConst", variableDeclarationStatement.IsConst);
        template.Add("type", Translate(variableDeclarationStatement.Type, ns, ast));
        template.Add("name", Translate(variableDeclarationStatement.Name, ns, ast));

        if (variableDeclarationStatement.Initializer != null)
            template.Add("initializer", Translate(variableDeclarationStatement.Initializer!, ns, ast));

        template.Add("isArray", variableDeclarationStatement.Type.IsArray);
        template.Add("arraySize", variableDeclarationStatement.Type.ArraySize);

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(PermuteStatement permuteStatement, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("permute_block");
        template.Add("condition", Translate(permuteStatement.Condition, ns, ast));
        template.Add("block", Translate(permuteStatement.Block, ns, ast));

        if (permuteStatement.Else != null && permuteStatement.Else.Children.Count > 0)
            template.Add("otherwise", Translate(permuteStatement.Else, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(NamespacedReference reference, Namespace ns, AST ast)
    {
        StringBuilder output = new();

        var name = $"'{reference.Name}'";

        if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR2B))
            output.Append("bool2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR2F))
            output.Append("float2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR2I))
            output.Append("int2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR2UI))
            output.Append("uint2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR3B))
            output.Append("bool3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR3F))
            output.Append("float3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR3I))
            output.Append("int3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR3UI))
            output.Append("uint3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR4B))
            output.Append("bool4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR4F))
            output.Append("float4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR4I))
            output.Append("int4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_VECTOR4UI))
            output.Append("uint4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX2F))
            output.Append("float2x2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX3F))
            output.Append("float3x3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX4F))
            output.Append("float4x4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX2X3F))
            output.Append("float2x3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX2X4F))
            output.Append("float2x4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX3X2F))
            output.Append("float3x2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX3X4F))
            output.Append("float3x4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX4X2F))
            output.Append("float4x2");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_MATRIX4X3F))
            output.Append("float4x3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_COLOR3))
            output.Append("float3");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_COLOR4))
            output.Append("float4");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_SAMPLER))
            output.Append("SamplerState");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_TEXTURE1D))
            output.Append("Texture1D");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_TEXTURE2D))
            output.Append("Texture2D");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_TEXTURE1DARRAY))
            output.Append("Texture1DArray");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_TEXTURE2DARRAY))
            output.Append("Texture2DArray");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_TEXTURE3D))
            output.Append("Texture3D");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_CUBEMAP))
            output.Append("TextureCube");
        else if (name == SPSLLexer.DefaultVocabulary.GetLiteralName(SPSLLexer.TYPE_CUBEMAPARRAY))
            output.Append("TextureCubeArray");
        else
        {
            INamespaceChild? type = Resolve(reference.Name, ns, ast);
            if (type is Type spslType)
                output.Append(spslType.Kind == TypeKind.Enum ? "uint" : spslType.Name);
            else
                output.Append(reference.Name.Replace(':', '_'));
        }

        return output.ToString();
    }
}
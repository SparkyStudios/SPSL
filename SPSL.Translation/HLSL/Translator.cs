using System.Text;
using Antlr4.StringTemplate;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Utils;
using SPSL.Translation.Common;
using static SPSL.Language.Core.SPSLLexer;
using Stream = SPSL.Language.Parsing.AST.Stream;
using Type = SPSL.Language.Parsing.AST.Type;

namespace SPSL.Translation.HLSL;

public class TranslatorConfiguration
{
    public IEnumerable<NamespacedReference>? Shaders { get; set; }
    public IEnumerable<NamespacedReference>? Namespaces { get; set; }
    public Dictionary<string, string> Permutations { get; set; } = new();
}

public class Translator
{
    private static string TranslateOp(Op op)
    {
        return op switch
        {
            Op.Unknown => "/* <spsl-error>Unknown operator</spsl-error> */",
            Op.Pipe => "|",
            Op.Ampersand => "&",
            Op.Plus => "+",
            Op.Minus => "-",
            Op.Star => "*",
            Op.Exponent => "**",
            Op.Modulo => "%",
            Op.Div => "/",
            Op.Assignment => "=",
            Op.Equals => "==",
            Op.NotEquals => "!=",
            Op.LessThan => "<",
            Op.GreaterThan => ">",
            Op.LessThanOrEqual => "<=",
            Op.GreaterThanOrEqual => ">=",
            Op.Increment => "++",
            Op.Decrement => "--",
            Op.PlusAssignment => "+=",
            Op.MinusAssignment => "-=",
            Op.AsteriskAssignment => "*=",
            Op.DivideAssignment => "/=",
            Op.ModuloAssignment => "%=",
            Op.BitwiseOrAssignment => "|=",
            Op.BitwiseAndAssignment => "&=",
            Op.ExponentAssignment => "^=",
            Op.LeftShiftAssignment => "<<=",
            Op.RightShiftAssignment => ">>=",
            Op.Or => "||",
            Op.And => "&&",
            Op.Not => "!",
            Op.Xor => "^^",
            Op.LeftShift => "<<",
            Op.RightShift => ">>",
            _ => throw new ArgumentOutOfRangeException(nameof(op), op, "Unknown operator,")
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

    private static INamespaceChild? Resolve(string root, Namespace ns, Ast ast)
    {
        if (root == string.Empty)
            return null;

        int pos = root.IndexOf(Namespace.Separator, StringComparison.Ordinal);

        if (pos < 0)
        {
            if (ns.GetChild(root) is { } namespaceChild)
                return namespaceChild;

            return ast
                .Select(n => n.FullName != ns.FullName && n.GetChild(root) is { } c ? c : null)
                .FirstOrDefault(c => c is not null);
        }

        if (ns.FullName != string.Empty && root.StartsWith(ns.FullName))
            return ns.GetChild(root[ns.FullName.Length..].TrimStart(Namespace.SeparatorChar));

        return (
            from n in ast.Where(n => n.FullName != ns.FullName)
            where root.StartsWith(n.FullName)
            select n.GetChild(root[n.FullName.Length..].TrimStart(Namespace.SeparatorChar))
        ).FirstOrDefault();
    }

    private const string ShaderStreamPosKey = "__SPSL_FINAL_STREAM_OUTPUT__";

    private readonly HashSet<string> _processedFragments = new();
    private readonly HashSet<string> _processedPermutations = new();

    private readonly TemplateGroupString _hlslTemplate;

    private string _currentBase = string.Empty;
    private Shader _currentShader = null!;
    private readonly Stream _shaderStream = new(new() { Value = "ShaderStream" }, Array.Empty<StreamProperty>());

    private readonly TranslatorConfiguration _configuration;

    public Stream ShaderStream => _shaderStream;

    private string TranslateShaderFragment(NamespacedReference name, Namespace ns, Ast ast,
        IDictionary<int, uint>? conflicts, IEnumerable<IBlockChild>? overriddenFunctions)
    {
        StringBuilder output = new();

        INamespaceChild? child = Resolve(name.Name, ns, ast);
        if (child is ShaderFragment fragment)
        {
            if (!_processedFragments.Contains(new(fragment.GetFullName())))
            {
                output.AppendLine(Translate(fragment, ns, ast, overriddenFunctions, conflicts));
                _processedFragments.Add(new(fragment.GetFullName()));
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

    public Translator(TranslatorConfiguration configuration)
    {
        _configuration = configuration;

        TemplateGroupString template;

        const string hlslTemplate = "SPSL.Translation.Templates.HLSL.stg";
        const string baseTemplate = "SPSL.Translation.Templates.Base.stg";

        using (System.IO.Stream? stream = GetType().Assembly.GetManifestResourceStream(hlslTemplate))
        using (StreamReader reader = new(stream!))
            _hlslTemplate = new("HLSL", reader.ReadToEnd());

        using (System.IO.Stream? stream = GetType().Assembly.GetManifestResourceStream(baseTemplate))
        using (StreamReader reader = new(stream!))
            template = new("Base", reader.ReadToEnd());

        _hlslTemplate.ImportTemplates(template);
    }

    public string Translate(Ast ast)
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
            if (_configuration.Namespaces is not null && _configuration.Namespaces.All(n => n.Name != ns.FullName))
                continue;

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

    public string Translate(Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        foreach (INamespaceChild namespaceChild in ns.Where(child => child is PermutationVariable))
        {
            output.AppendLine();

            var child = (PermutationVariable)namespaceChild;
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

        var shaders = ns.Where(child =>
            child is Shader { IsAbstract: false } shader &&
            (_configuration.Shaders is null || !_configuration.Shaders.Any() ||
             _configuration.Shaders.Any(s =>
                 s.Equals(shader.GetReference()))));

        if (shaders.Any())
        {
            output.AppendLine();
            output.AppendLine(ShaderStreamPosKey);
        }

        foreach (INamespaceChild namespaceChild in shaders)
        {
            var child = (Shader)namespaceChild;
            output.Append(Translate(child, ns, ast));
        }

        output.Replace(ShaderStreamPosKey, Translate(_shaderStream, ns, ast));

        return output.ToString();
    }

    public string Translate(PermutationVariable permutationVariable, Namespace ns, Ast ast)
    {
        if (_processedPermutations.Contains(permutationVariable.GetFullName()))
            return string.Empty;

        StringBuilder output = new();

        string name = Translate(permutationVariable.Name, ns, ast);

        Template template = _hlslTemplate.GetInstanceOf("permutation_var");
        template.Add("m",
            new Macro(name,
                _configuration.Permutations.TryGetValue(name, out string? value)
                    ? value
                    : Translate(permutationVariable.Initializer, ns, ast)));

        if (permutationVariable.Type == PermutationVariableType.Enum)
        {
            for (var i = 0; i < permutationVariable.EnumerationValues.Length; i++)
            {
                Identifier item = permutationVariable.EnumerationValues[i];
                template.Add("enum_values", new Macro(item.Value, i.ToString()));
            }
        }

        _processedPermutations.Add(permutationVariable.GetFullName());

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(Identifier identifier, Namespace ns, Ast ast)
    {
        return identifier.Value;
    }

    public string Translate(GlobalVariable global, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        // TODO: Handle name conflicts between namespaces

        Template template = _hlslTemplate.GetInstanceOf("const_global_var");
        template.Add("isStatic", global.IsStatic);
        template.Add("type", Translate(global.Type, ns, ast));
        template.Add("name", Translate(global.Name, ns, ast));
        template.Add("initializer", Translate(global.Initializer, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(Type type, Namespace ns, Ast ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        switch (type.Kind)
        {
            case TypeKind.Struct:
            {
                Template template = _hlslTemplate.GetInstanceOf("struct");
                template.Add("name", Translate(type.Name, ns, ast));
                foreach (TypeProperty property in type.Properties)
                    template.Add("properties", new Prop(Translate(property.Type, ns, ast), property.Name.Value));
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
                            member.Name.Value,
                            member.Initializer != null ? Translate(member.Initializer, ns, ast) : null
                        )
                    );
                }

                output.Append(template.Render());
                break;
            }
            case TypeKind.Unknown:
            default:
                output.AppendLine("// <spsl-error>");
                output.AppendLine(
                    $"// Unknown Type: {type.Name}. Make sure the syntax is correct (either 'type TypeName as struct' or 'type TypeName as enum'.");
                output.AppendLine("// </spsl-error>");
                output.AppendLine();
                break;
        }

        return output.ToString();
    }

    public string Translate(ShaderFragment fragment, Namespace ns, Ast ast,
        IEnumerable<IBlockChild>? shouldOverride = null, IDictionary<int, uint>? conflicts = null)
    {
        StringBuilder output = new();
        HashSet<IBlockChild> overriddenChildren = new();

        foreach (IBlockChild child in fragment.Children)
            if (child is ShaderFunction { IsOverride: true } /* or GlobalVariable*/)
                overriddenChildren.Add(child);

        if (fragment.ExtendedShaderFragment != NamespacedReference.Null)
            output.Append(TranslateShaderFragment(fragment.ExtendedShaderFragment, ns, ast, conflicts,
                overriddenChildren));

        if (fragment.ImportedShaderFragments.Count > 0)
        {
            IDictionary<int, uint> fragmentChildren = conflicts ?? new Dictionary<int, uint>();

            foreach (NamespacedReference import in fragment.ImportedShaderFragments)
            {
                if (import == NamespacedReference.Null)
                    continue;

                INamespaceChild? frag = Resolve(import.Name, ns, ast);
                if (frag is not ShaderFragment shaderFragment)
                    continue;

                foreach (IBlockChild child in shaderFragment.Children)
                {
                    int code = child.Name.GetSemanticHashCode();

                    if (child is ShaderFunction function)
                    {
                        code = function.Function.Head.GetSemanticHashCode();
                    }

                    if (fragmentChildren.ContainsKey(code))
                        fragmentChildren[code] += 1;
                    else
                        fragmentChildren.Add(code, 1);
                }
            }

            foreach (NamespacedReference import in fragment.ImportedShaderFragments)
            {
                if (import == NamespacedReference.Null)
                    continue;

                output.Append(TranslateShaderFragment(import, ns, ast, fragmentChildren, overriddenChildren));
            }
        }

        foreach (Stream stream in fragment.Streams)
        foreach (StreamProperty property in stream.Properties)
            _shaderStream.Properties.Add(property);

        _currentBase = Translate(fragment.ExtendedShaderFragment, ns, ast);

        IEnumerable<IBlockChild>? blockChildren = shouldOverride as HashSet<IBlockChild> ?? shouldOverride?.ToHashSet();

        foreach (PermutationVariable permutation in fragment.PermutationVariables)
        {
            output.AppendLine();

            Identifier name = permutation.Name;
            if (blockChildren?.SingleOrDefault(m =>
                    m is PermutationVariable permutationVariable &&
                    permutationVariable.Name.SemanticallyEquals(permutation.Name)) != null ||
                (conflicts is not null && conflicts.TryGetValue(name.GetSemanticHashCode(), out uint value) &&
                 value > 1))
                permutation.Name = new()
                {
                    Value = $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(permutation.Name, ns, ast)}"
                };

            output.Append(Translate(permutation, ns, ast));
        }

        foreach (GlobalVariable variable in fragment.GlobalVariables)
        {
            output.AppendLine();

            Identifier name = variable.Name;
            if (blockChildren?.SingleOrDefault(m =>
                    m is GlobalVariable globalVariable && globalVariable.Name.SemanticallyEquals(variable.Name)) !=
                null ||
                (conflicts is not null && conflicts.TryGetValue(name.GetSemanticHashCode(), out uint value) &&
                 value > 1))
                variable.Name = new()
                    { Value = $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(variable.Name, ns, ast)}" };

            output.Append(Translate(variable, ns, ast));
        }

        foreach (Type type in fragment.Types)
        {
            output.AppendLine();

            Identifier name = type.Name;
            if (blockChildren?.SingleOrDefault(m =>
                    m is Type shaderType &&
                    shaderType.Name.SemanticallyEquals(type.Name)) != null ||
                (conflicts is not null && conflicts.TryGetValue(name.GetSemanticHashCode(), out uint value) &&
                 value > 1))
                type.Name = new()
                    { Value = $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(type.Name, ns, ast)}" };

            output.Append(Translate(type, ns, ast));
        }

        foreach (StructuredBuffer buffer in fragment.StructuredBuffers)
        {
            output.AppendLine();

            Identifier name = buffer.Name;
            if (blockChildren?.SingleOrDefault(m =>
                    m is StructuredBuffer shaderBuffer &&
                    shaderBuffer.Name.SemanticallyEquals(buffer.Name)) != null ||
                (conflicts is not null && conflicts.TryGetValue(name.GetSemanticHashCode(), out uint value) &&
                 value > 1))
                buffer.Name = new()
                    { Value = $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(buffer.Name, ns, ast)}" };

            output.Append(Translate(buffer, ns, ast));
        }

        foreach (TypedBuffer buffer in fragment.TypedBuffers)
        {
            output.AppendLine();

            Identifier name = buffer.Name;
            if (blockChildren?.SingleOrDefault(m =>
                    m is TypedBuffer shaderBuffer &&
                    shaderBuffer.Name.SemanticallyEquals(buffer.Name)) != null ||
                (conflicts is not null && conflicts.TryGetValue(name.GetSemanticHashCode(), out uint value) &&
                 value > 1))
                buffer.Name = new()
                    { Value = $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(buffer.Name, ns, ast)}" };

            output.Append(Translate(buffer, ns, ast));
        }

        foreach (ShaderFunction function in fragment.Functions)
        {
            output.AppendLine();

            int code = function.Function.Head.GetSemanticHashCode();
            if (blockChildren?.SingleOrDefault(m =>
                    m is ShaderFunction shaderFunction &&
                    shaderFunction.Function.Head.SemanticallyEquals(function.Function.Head)) != null ||
                (conflicts is not null && conflicts.TryGetValue(code, out uint value) && value > 1))
                function.Function.Head.Name = new()
                {
                    Value =
                        $"{Translate(fragment.GetReference(), ns, ast)}__{Translate(function.Function.Head.Name, ns, ast)}"
                };

            output.Append(Translate(function, ns, ast));
        }

        return output.ToString();
    }

    public enum ShaderTranslationStage
    {
        Full,
        Fragments,
        Types,
        FunctionDeclarations,
        Streams,
        FunctionDefinitions
    }

    public string Translate
    (
        Shader shader,
        Namespace ns,
        Ast ast,
        IEnumerable<IBlockChild>? shouldOverride = null,
        ShaderTranslationStage stage = ShaderTranslationStage.Full
    )
    {
        // 1. Process base shaders recursively
        //    - Rename overridden methods with BaseShader_MethodName
        // 2. Process shader fragments recursively
        //    - Latest shader fragment functions got predominance on previous ones if they have the same signature.
        // 3. Process this shader
        //    - Calls to this are removed
        //    - Calls to base are renamed with the parent shader name

        StringBuilder output = new();

        Shader? parent = null;
        if (shader.ExtendedShader != NamespacedReference.Null)
        {
            parent = Resolve(shader.ExtendedShader.Name, ns, ast) as Shader;
            if (parent is null)
            {
                output.AppendLine("// <spsl-error>");
                output.AppendLine(
                    $"// Shader not found: {shader.ExtendedShader.Name}. Make sure it exists or his namespace is imported.");
                output.AppendLine("// </spsl-error>");
                output.AppendLine();
                return string.Empty;
            }
        }

        HashSet<IBlockChild> overriddenChildren = new();

        foreach (IBlockChild child in shader.Children)
            if (child is ShaderFunction { IsOverride: true } function)
                overriddenChildren.Add(function);

        IEnumerable<IBlockChild>? blockChildren = shouldOverride as HashSet<IBlockChild> ?? shouldOverride?.ToHashSet();

        // Imported shader fragments
        if (stage == ShaderTranslationStage.Fragments)
        {
            if (parent != null)
                output.Append(Translate(parent, ns, ast, overriddenChildren, ShaderTranslationStage.Fragments));

            UpdateScope();
            if (shader.ImportedShaderFragments.Count > 0)
            {
                Dictionary<int, uint> fragmentChildren = new();

                foreach (IBlockChild child in shader.Children)
                {
                    int code = child.Name.GetSemanticHashCode();

                    if (child is ShaderFunction function)
                    {
                        code = function.Function.Head.GetSemanticHashCode();
                    }

                    if (fragmentChildren.ContainsKey(code))
                        fragmentChildren[code] += 1;
                    else
                        fragmentChildren.Add(code, 1);
                }

                foreach (NamespacedReference fragment in shader.ImportedShaderFragments)
                {
                    if (fragment == NamespacedReference.Null)
                        continue;

                    INamespaceChild? frag = Resolve(fragment.Name, ns, ast);
                    if (frag is not ShaderFragment shaderFragment) continue;

                    foreach (IBlockChild child in shaderFragment.Children)
                    {
                        int code = child.Name.GetSemanticHashCode();

                        if (child is ShaderFunction function)
                        {
                            code = function.Function.Head.GetSemanticHashCode();
                        }

                        if (fragmentChildren.ContainsKey(code))
                            fragmentChildren[code] += 1;
                        else
                            fragmentChildren.Add(code, 1);
                    }
                }

                foreach (NamespacedReference fragment in shader.ImportedShaderFragments)
                {
                    if (fragment == NamespacedReference.Null)
                        continue;

                    output.Append(TranslateShaderFragment(fragment, ns, ast, fragmentChildren, overriddenChildren));
                }
            }
        }

        // Shader members
        if (stage == ShaderTranslationStage.Types)
        {
            if (parent != null)
                output.Append(Translate(parent, ns, ast, overriddenChildren, ShaderTranslationStage.Types));

            UpdateScope();
            foreach (IBlockChild child in shader.Children)
            {
                string code = child switch
                {
                    GlobalVariable v => Translate(v, ns, ast),
                    TypedBuffer tb => Translate(tb, ns, ast),
                    StructuredBuffer sb => Translate(sb, ns, ast),
                    Type t => Translate(t, ns, ast),
                    // TODO: Sampler states
                    _ => string.Empty,
                };

                output.AppendLine(code);
            }
        }

        // Functions declaration
        if (stage == ShaderTranslationStage.FunctionDeclarations)
        {
            if (parent != null)
                output.Append(Translate(parent, ns, ast, overriddenChildren,
                    ShaderTranslationStage.FunctionDeclarations));

            _currentShader = shader;
            foreach (IBlockChild child in shader.Children)
            {
                if (child is not ShaderFunction shaderFunction)
                    continue;

                if (blockChildren?.SingleOrDefault(m =>
                        m is ShaderFunction sf && sf.Function.Head.SemanticallyEquals(shaderFunction.Function.Head)) !=
                    null)
                    shaderFunction.Function.Head.Name = new()
                    {
                        Value =
                            $"{Translate(shader.GetReference(), ns, ast)}__{Translate(shaderFunction.Function.Head.Name, ns, ast)}"
                    };

                Annotation? entry = shaderFunction.Annotations.SingleOrDefault(annotation => annotation.IsEntry);

                if (entry != null || shaderFunction.Name.Value == _currentShader.Name.Value &&
                    shaderFunction.IsConstructor)
                {
                    switch (_currentShader.Stage)
                    {
                        case ShaderStage.Compute:
                        {
                            // -- Ensure that we have all our compute shader keywords as parameters
                            shaderFunction.Function.Head.Signature.Arguments.Clear();

                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.Unspecified,
                                    new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                    new() { Value = "sp_GroupThreadId : SV_GroupThreadID" }
                                )
                            );

                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.Unspecified,
                                    new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                    new() { Value = "sp_GroupId : SV_GroupID" }
                                )
                            );

                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.Unspecified,
                                    new BuiltInDataType(BuiltInDataTypeKind.Vector3ui),
                                    new() { Value = "sp_DispatchThreadId : SV_DispatchThreadID" }
                                )
                            );

                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.Unspecified,
                                    new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger),
                                    new() { Value = "sp_GroupIndex : SV_GroupIndex" }
                                )
                            );

                            break;
                        }
                        case ShaderStage.Vertex:
                        {
                            // Vertex shader entry point always returns a TransientStream struct
                            shaderFunction.Function.Head.ReturnType =
                                new UserDefinedDataType(new(new Identifier { Value = "TransientStream" }));

                            var outStreamDecl = new VariableDeclarationStatement
                            (
                                type: new UserDefinedDataType(new(new Identifier { Value = "TransientStream" })),
                                name: new() { Value = "out_streams" }
                            );

                            var outStreamRet =
                                new ReturnStatement(new BasicExpression(new() { Value = "out_streams" }));

                            if (shaderFunction.Function.Body.Children.First() is not VariableDeclarationStatement
                                {
                                    Name.Value: "out_streams"
                                })
                                shaderFunction.Function.Body.Children.Prepend(outStreamDecl);

                            if (shaderFunction.Function.Body.Children.Last() is not ReturnStatement
                                {
                                    Expression: BasicExpression { Identifier.Value: "out_streams" }
                                })
                                shaderFunction.Function.Body.Children.Append(outStreamRet);

                            // Vertex shader entry point always takes as arguments a single InputStream struct
                            shaderFunction.Function.Head.Signature.Arguments.Clear();
                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.In,
                                    new UserDefinedDataType(new(new Identifier { Value = "InputStream" })),
                                    new() { Value = "in_streams" }
                                )
                            );
                            break;
                        }
                        case ShaderStage.Pixel:
                        {
                            // Pixel shader entry point always returns a OutputStream struct
                            shaderFunction.Function.Head.ReturnType =
                                new UserDefinedDataType(new(new Identifier { Value = "OutputStream" }));

                            var outStreamDecl = new VariableDeclarationStatement
                            (
                                type: new UserDefinedDataType(new(new Identifier { Value = "OutputStream" })),
                                name: new() { Value = "out_streams" }
                            );

                            var outStreamRet =
                                new ReturnStatement(new BasicExpression(new() { Value = "out_streams" }));

                            if (shaderFunction.Function.Body.Children.First() is not VariableDeclarationStatement
                                {
                                    Name.Value: "out_streams"
                                })
                                shaderFunction.Function.Body.Children.Prepend(outStreamDecl);

                            if (shaderFunction.Function.Body.Children.Last() is not ReturnStatement
                                {
                                    Expression: BasicExpression { Identifier.Value: "out_streams" }
                                })
                                shaderFunction.Function.Body.Children.Append(outStreamRet);

                            // Pixel shader entry point always takes as arguments a single TransientStream struct
                            shaderFunction.Function.Head.Signature.Arguments.Clear();
                            shaderFunction.Function.Head.Signature.Arguments.Add
                            (
                                new
                                (
                                    DataFlow.In,
                                    new UserDefinedDataType(new(new Identifier { Value = "TransientStream" })),
                                    new() { Value = "in_streams" }
                                )
                            );
                            break;
                        }
                    }
                }

                output.Append(Translate(shaderFunction.Function.Head, ns, ast));
                output.AppendLine(";");
            }
        }

        // Streams
        if (stage == ShaderTranslationStage.Streams)
        {
            if (parent != null)
                output.Append(Translate(parent, ns, ast, overriddenChildren, ShaderTranslationStage.Streams));

            UpdateScope();
            foreach (Stream stream in shader.Children.OfType<Stream>())
            foreach (StreamProperty property in stream.Properties)
                _shaderStream.Properties.Add(property);
        }

        // Functions definition
        if (stage == ShaderTranslationStage.FunctionDefinitions)
        {
            output.AppendLine();

            if (parent != null)
                output.Append(
                    Translate(parent, ns, ast, overriddenChildren, ShaderTranslationStage.FunctionDefinitions));

            UpdateScope();
            foreach (IBlockChild child in shader.Children)
            {
                if (child is ShaderFunction shaderFunction)
                    if (blockChildren?.SingleOrDefault(m =>
                            m is ShaderFunction sf &&
                            sf.Function.Head.SemanticallyEquals(shaderFunction.Function.Head)) != null)
                        shaderFunction.Function.Head.Name = new()
                        {
                            Value =
                                $"{Translate(shader.GetReference(), ns, ast)}__{Translate(shaderFunction.Function.Head.Name, ns, ast)}"
                        };

                string code = child switch
                {
                    Function function => Translate(function, ns, ast),
                    ShaderFunction sf => Translate(sf, ns, ast),
                    _ => string.Empty,
                };

                output.AppendLine(code);
            }
        }

        if (stage == ShaderTranslationStage.Full)
        {
            output.Append(Translate(shader, ns, ast, shouldOverride, ShaderTranslationStage.Fragments));
            output.Append(Translate(shader, ns, ast, shouldOverride, ShaderTranslationStage.Types));
            output.Append(Translate(shader, ns, ast, shouldOverride, ShaderTranslationStage.FunctionDeclarations));
            output.Append(Translate(shader, ns, ast, shouldOverride, ShaderTranslationStage.Streams));
            output.Append(Translate(shader, ns, ast, shouldOverride, ShaderTranslationStage.FunctionDefinitions));
        }

        return output.ToString();

        void UpdateScope()
        {
            _currentShader = shader;
            _currentBase = Translate(shader.ExtendedShader, ns, ast);
        }
    }

    public string Translate(IShaderMember member, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = member switch
        {
            StructuredBuffer buffer => Translate(buffer, ns, ast),
            TypedBuffer buffer => Translate(buffer, ns, ast),
            Type type => Translate(type, ns, ast),
            _ => string.Empty,
        };

        output.Append(code);

        return output.ToString();
    }

    public string Translate(StructuredBuffer buffer, Namespace ns, Ast ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        Template? template;

        switch (buffer.Access)
        {
            case BufferAccess.Constant:
                template = _hlslTemplate.GetInstanceOf("cbuffer");
                break;
            case BufferAccess.WriteOnly or BufferAccess.ReadWrite:
                template = _hlslTemplate.GetInstanceOf("rwsbuffer");
                template.Add("isCoherent", (buffer.Storage & BufferStorage.Coherent) == BufferStorage.Coherent);
                break;
            default:
                template = _hlslTemplate.GetInstanceOf("sbuffer");
                break;
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

        template.Add("name", Translate(buffer.Name, ns, ast));

        string? binding = null;
        string? set = null;

        foreach (Annotation annotation in buffer.Annotations)
        {
            if (annotation.Identifier.Value != "register")
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
            template.Add("properties", new Prop(Translate(property.Type, ns, ast), property.Name.Value));

        output.Append(template.Render());

        return output.ToString();
    }

    public string Translate(TypedBuffer buffer, Namespace ns, Ast ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        Template? template;

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

        switch (buffer.Access)
        {
            case BufferAccess.Constant:
                output.Clear();

                output.AppendLine("// <spsl-error>");
                output.AppendLine("// An SPSL TypedBuffer cannot be constant. Failed buffer name: " + buffer.Name);
                output.AppendLine("// </spsl-error>");
                output.AppendLine();

                return output.ToString();
            case BufferAccess.WriteOnly:
            case BufferAccess.ReadWrite:
                template = _hlslTemplate.GetInstanceOf("rwtbuffer");
                template.Add("isCoherent", (buffer.Storage & BufferStorage.Coherent) == BufferStorage.Coherent);
                template.Add("isStructured", isStructured);
                break;
            case BufferAccess.ReadOnly:
            default:
                template = _hlslTemplate.GetInstanceOf("tbuffer");
                template.Add("isStructured", isStructured);
                break;
        }

        template.Add("name", Translate(buffer.Name, ns, ast));
        template.Add("type", Translate(buffer.DataType, ns, ast));

        string? binding = null;
        string? set = null;

        foreach (Annotation annotation in buffer.Annotations)
        {
            if (annotation.Identifier.Value != "register")
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

    public string Translate(Stream stream, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        void TranslateProperty(StreamProperty property, Template template)
        {
            string? semantic = null;
            Annotation? annotation = property.Annotations.SingleOrDefault(a => a.IsSemantic);

            if (annotation?.Identifier.Value == "semantic")
            {
                IExpression? expression = annotation?.Arguments.SingleOrDefault();
                semantic = expression is not null ? Translate(expression, ns, ast) : null;
            }
            else
            {
                semantic = annotation?.Identifier.Value switch
                {
                    "position" => "POSITION",
                    "texcoord" => "TEXCOORD",
                    "normal" => "NORMAL",
                    "tangent" => "TANGENT",
                    "bitangent" => "BITANGENT",
                    "color" => "COLOR",
                    "boneweights" => "BONEWEIGHTS",
                    "boneindices" => "BONEINDICES",
                    _ => null
                };

                IExpression? expression = annotation?.Arguments.SingleOrDefault();
                semantic += expression is not null ? Translate(expression, ns, ast) : string.Empty;
            }

            template.Add("properties",
                new Prop(Translate(property.Type, ns, ast), property.Name.Value, null, semantic));
        }

        // Vertex Shader Input
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", "InputStream");
            foreach (StreamProperty property in stream.Inputs)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        // Transient Shader Data / Pixel Shader Input
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", "TransientStream");
            foreach (StreamProperty property in stream.Transients)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        // Pixel Shader Output
        {
            output.AppendLine();
            Template template = _hlslTemplate.GetInstanceOf("struct");
            template.Add("name", "OutputStream");
            foreach (StreamProperty property in stream.Outputs)
                TranslateProperty(property, template);

            output.Append(template.Render());
        }

        return output.ToString();
    }

    public string Translate(ShaderFunction function, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Annotation? entry = function.Annotations.SingleOrDefault(annotation => annotation.IsEntry);

        if (entry != null || function.Name == _currentShader.Name && function.IsConstructor)
        {
            if (_currentShader.Stage == ShaderStage.Compute)
            {
                // -- Compute shader entry points need the numthreads attribute

                output.AppendLine();
                output.Append(
                    $"[[numthreads({_currentShader.ComputeParams.ThreadCountX}, {_currentShader.ComputeParams.ThreadCountY}, {_currentShader.ComputeParams.ThreadCountZ})]]");
            }
        }

        output.Append(Translate(function.Function, ns, ast));

        return output.ToString();
    }

    public string Translate(Function function, Namespace ns, Ast ast)
    {
        StringBuilder output = new();
        output.AppendLine();

        output.AppendLine(Translate(function.Head, ns, ast));
        output.AppendLine("{");
        output.AppendLine(Translate(function.Body, ns, ast));
        output.AppendLine("}");

        return output.ToString();
    }

    public string Translate(FunctionHead head, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("func_head");
        template.Add("type", Translate(head.ReturnType, ns, ast));
        template.Add("name", Translate(head.Name, ns, ast));
        foreach (FunctionArgument arg in head.Signature.Arguments)
        {
            template.Add("args", new Arg(arg.Flow switch
            {
                DataFlow.In => "in",
                DataFlow.InOut => "inout",
                DataFlow.Out => "out",
                DataFlow.Const => "const",
                _ => null
            }, Translate(arg.Type, ns, ast), arg.Name.Value));
        }

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(IDataType dataType, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = dataType switch
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

    public string Translate(BuiltInDataType builtInType, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(TranslateBuiltInType(builtInType.Type));

        return output.ToString();
    }

    public string Translate(PrimitiveDataType primitiveType, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(TranslatePrimitiveType(primitiveType.Type));

        return output.ToString();
    }

    public string Translate(UnknownDataType unknownType, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append("__unknown_type__");

        return output.ToString();
    }

    public string Translate(UserDefinedDataType userDefinedType, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(Translate(userDefinedType.Type, ns, ast));

        return output.ToString();
    }

    public string Translate(IExpression expression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = expression switch
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

    public string Translate(IConstantExpression constantExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = constantExpression switch
        {
            IPrimitiveExpression primitiveExpression => Translate(primitiveExpression, ns, ast),
            UserDefinedConstantExpression userDefinedConstantExpression => Translate(userDefinedConstantExpression, ns,
                ast),
            _ => string.Empty,
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(IPrimitiveExpression primitiveExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = primitiveExpression switch
        {
            ILiteral literal => Translate(literal, ns, ast),
            _ => string.Empty
        };

        output.Append(code);
        return output.ToString();
    }

    public string Translate(ILiteral literal, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        string code = literal switch
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

    public string Translate(BoolLiteral boolLiteral, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(boolLiteral.Value.ToString().ToLower());

        return output.ToString();
    }

    public string Translate(DoubleLiteral doubleLiteral, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(doubleLiteral.Value);

        return output.ToString();
    }

    public string Translate(FloatLiteral floatLiteral, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(floatLiteral.Value);

        if (Math.Abs((int)floatLiteral.Value - floatLiteral.Value) == 0)
            output.Append(".0");

        output.Append('f');

        return output.ToString();
    }

    public string Translate(IntegerLiteral integerLiteral, Namespace ns, Ast ast)
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

    public string Translate(StringLiteral literal, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(literal.Value);

        return output.ToString();
    }

    public string Translate(UnsignedIntegerLiteral unsignedIntegerLiteral, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(unsignedIntegerLiteral.Value);

        return output.ToString();
    }

    public string Translate(UserDefinedConstantExpression userDefinedConstantExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(Translate(userDefinedConstantExpression.Identifier, ns, ast));

        return output.ToString();
    }

    public string Translate(ArrayAccessExpression arrayAccessExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("array_access");
        template.Add("var", Translate(arrayAccessExpression.Expression, ns, ast));
        template.Add("idx", Translate(arrayAccessExpression.ArrayIndex, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(AssignmentExpression assignmentExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("binary_op");
        template.Add("lhs", Translate(assignmentExpression.Left, ns, ast));
        template.Add("op", TranslateOp(assignmentExpression.Operator));
        template.Add("rhs", Translate(assignmentExpression.Right, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(BasicExpression basicExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(basicExpression.Identifier.Value);

        return output.ToString();
    }

    public string Translate(BinaryOperationExpression binaryExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template;

        // Use pow function instead
        if (binaryExpression.Operator == Op.Exponent)
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

    public string Translate(CastExpression castExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("cast");
        template.Add("type", Translate(castExpression.Type, ns, ast));
        template.Add("value", Translate(castExpression.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(InvocationExpression invocationExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("invocation");
        template.Add("function", Translate(invocationExpression.Name, ns, ast));

        foreach (InvocationParameter parameter in invocationExpression.Parameters)
            template.Add("params", Translate(parameter.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(ChainedExpression chainedMemberReferenceExpression, Namespace ns, Ast ast)
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

    public string Translate(MethodMemberReferenceExpression methodMemberReferenceExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template;

        switch (methodMemberReferenceExpression.Target.Target.Value)
        {
            // Calls to this are ignored
            case "this":
                template = _hlslTemplate.GetInstanceOf("invocation");
                template.Add("function", Translate(methodMemberReferenceExpression.Member.Name, ns, ast));
                break;

            // Calls to base are renamed
            case "base":
                template = _hlslTemplate.GetInstanceOf("invocation");
                template.Add("function",
                    $"{_currentBase}__{Translate(methodMemberReferenceExpression.Member.Name, ns, ast)}");
                break;

            default:
                template = _hlslTemplate.GetInstanceOf("member_invocation");
                template.Add("owner", methodMemberReferenceExpression.Target);
                template.Add("function", Translate(methodMemberReferenceExpression.Member.Name, ns, ast));
                break;
        }

        foreach (InvocationParameter parameter in methodMemberReferenceExpression.Member.Parameters)
            template.Add("params", Translate(parameter.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(NegateOperationExpression negateOperationExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append('!');
        output.Append(Translate(negateOperationExpression.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(NewInstanceExpression newInstanceExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("new_instance");
        template.Add("type", Translate(newInstanceExpression.Type, ns, ast));

        foreach (InvocationParameter argument in newInstanceExpression.Parameters)
            template.Add("params", Translate(argument.Expression, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(ParenthesizedExpression parenthesizedExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append('(');
        output.Append(Translate(parenthesizedExpression.Expression, ns, ast));
        output.Append(')');

        return output.ToString();
    }

    public string Translate(PropertyMemberReferenceExpression propertyMemberReferenceExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        switch (propertyMemberReferenceExpression.Target.Target.Value)
        {
            // Calls to this are ignored
            case "this":
                output.Append(propertyMemberReferenceExpression.Member);
                break;

            // Calls to base are renamed
            case "base":
                output.Append($"{_currentBase}__{propertyMemberReferenceExpression.Member}");
                break;

            default:
                output.Append($"{propertyMemberReferenceExpression.Target}.{propertyMemberReferenceExpression.Member}");
                break;
        }

        return output.ToString();
    }

    public string Translate(TernaryOperationExpression ternaryOperationExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("ternary_op");
        template.Add("condition", Translate(ternaryOperationExpression.Condition, ns, ast));
        template.Add("lhs", Translate(ternaryOperationExpression.WhenTrue, ns, ast));
        template.Add("rhs", Translate(ternaryOperationExpression.WhenFalse, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(UnaryOperationExpression unaryExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("unary_op");
        template.Add("op", TranslateOp(unaryExpression.Operator));
        template.Add("expression", Translate(unaryExpression.Expression, ns, ast));
        template.Add("isPostfix", unaryExpression.IsPostfix);

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(SignedExpression signedExpression, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(signedExpression.Sign);
        output.Append(Translate(signedExpression.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(IStatement statement, Namespace ns, Ast ast)
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

    public string Translate(BreakStatement breakStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append("break");

        return output.ToString();
    }

    public string Translate(ContinueStatement continueStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append("continue");

        return output.ToString();
    }

    public string Translate(DiscardStatement discardStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append("discard");

        return output.ToString();
    }

    public string Translate(ExpressionStatement expressionStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        output.Append(Translate(expressionStatement.Expression, ns, ast));

        return output.ToString();
    }

    public string Translate(IfStatement ifStatement, Namespace ns, Ast ast)
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

    public string Translate(WhileStatement whileStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        if (whileStatement.IsDoWhile)
        {
            output.Append("do {");
            output.Append(Translate(whileStatement.Block, ns, ast));
            output.Append("} while(");
            output.Append(Translate(whileStatement.Condition, ns, ast));
            output.Append(");");
        }
        else
        {
            output.Append("while (");
            output.Append(Translate(whileStatement.Condition, ns, ast));
            output.AppendLine(")");
            output.Append(Translate(whileStatement.Block, ns, ast));
        }

        return output.ToString();
    }

    public string Translate(ReturnStatement ret, Namespace ns, Ast ast)
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

    public string Translate(StatementBlock body, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("statements_list");
        foreach (IStatement statement in body.Children)
            template.Add("stats", Translate(statement, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(StatementCollection statementCollection, Namespace ns, Ast ast)
    {
        StringBuilder output = new();
        var statements = statementCollection.Statements.Select(statement => Translate(statement, ns, ast));

        output.Append(string.Join('\n', statements));
        return output.ToString();
    }

    public string Translate(VariableDeclarationStatement variableDeclarationStatement, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        Template template = _hlslTemplate.GetInstanceOf("variable_declaration");
        template.Add("isConst", variableDeclarationStatement.IsConst);
        template.Add("type", Translate(variableDeclarationStatement.Type, ns, ast));
        template.Add("name", variableDeclarationStatement.Name.Value);

        if (variableDeclarationStatement.Initializer != null)
            template.Add("initializer", Translate(variableDeclarationStatement.Initializer!, ns, ast));

        template.Add("isArray", variableDeclarationStatement.Type.IsArray);
        template.Add("arraySize", variableDeclarationStatement.Type.ArraySize);

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(PermuteStatement permuteStatement, Namespace ns, Ast ast)
    {
        var expression = (BinaryOperationExpression)permuteStatement.Condition;
        var permutationName = (BasicExpression)expression.Left;
        var permutationValue = (IConstantExpression)expression.Right;

        StringBuilder output = new();

        if (_configuration.Permutations.TryGetValue(permutationName.Identifier.Value, out string? value))
        {
            if (value != DeclarationString.From(permutationValue))
            {
                return permuteStatement.Else is { Children.Count: > 0 }
                    ? Translate(permuteStatement.Else, ns, ast)
                    : string.Empty;
            }

            return Translate(permuteStatement.Block, ns, ast);
        }

        Template template = _hlslTemplate.GetInstanceOf("permute_block");
        template.Add("condition", Translate(permuteStatement.Condition, ns, ast));
        template.Add("block", Translate(permuteStatement.Block, ns, ast));

        if (permuteStatement.Else is { Children.Count: > 0 })
            template.Add("otherwise", Translate(permuteStatement.Else, ns, ast));

        output.Append(template.Render());
        return output.ToString();
    }

    public string Translate(NamespacedReference reference, Namespace ns, Ast ast)
    {
        StringBuilder output = new();

        var name = $"'{reference.Name}'";

        if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR2B))
            output.Append("bool2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR2F))
            output.Append("float2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR2D))
            output.Append("double2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR2I))
            output.Append("int2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR2UI))
            output.Append("uint2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR3B))
            output.Append("bool3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR3F))
            output.Append("float3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR3D))
            output.Append("double3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR3I))
            output.Append("int3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR3UI))
            output.Append("uint3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR4B))
            output.Append("bool4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR4F))
            output.Append("float4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR4D))
            output.Append("double4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR4I))
            output.Append("int4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_VECTOR4UI))
            output.Append("uint4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2F))
            output.Append("float2x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2D))
            output.Append("double2x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3F))
            output.Append("float3x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3D))
            output.Append("double3x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4F))
            output.Append("float4x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4D))
            output.Append("double4x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X3F))
            output.Append("float2x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X3D))
            output.Append("double2x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X4F))
            output.Append("float2x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X4D))
            output.Append("double2x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X2F))
            output.Append("float3x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X2D))
            output.Append("double3x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X4F))
            output.Append("float3x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X4D))
            output.Append("double3x4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X2F))
            output.Append("float4x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X2D))
            output.Append("double4x2");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X3F))
            output.Append("float4x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X3D))
            output.Append("double4x3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_COLOR3))
            output.Append("float3");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_COLOR4))
            output.Append("float4");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_SAMPLER))
            output.Append("SamplerState");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_TEXTURE1D))
            output.Append("Texture1D");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_TEXTURE2D))
            output.Append("Texture2D");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_TEXTURE1DARRAY))
            output.Append("Texture1DArray");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_TEXTURE2DARRAY))
            output.Append("Texture2DArray");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_TEXTURE3D))
            output.Append("Texture3D");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_CUBEMAP))
            output.Append("TextureCube");
        else if (name == DefaultVocabulary.GetLiteralName(TYPE_CUBEMAPARRAY))
            output.Append("TextureCubeArray");
        else
        {
            INamespaceChild? type = Resolve(reference.Name, ns, ast);
            if (type is Type spslType)
                output.Append(spslType.Kind == TypeKind.Enum ? "uint" : spslType.Name);
            else
                output.Append(reference.Name.Replace(Namespace.SeparatorChar, '_'));
        }

        return output.ToString();
    }
}
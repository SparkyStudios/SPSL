using System.Globalization;
using System.Text;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using static SPSL.Language.Core.SPSLParser;
using Stream = SPSL.Language.Parsing.AST.Stream;
using Type = SPSL.Language.Parsing.AST.Type;

namespace SPSL.Language.Utils;

/// <summary>
/// Gets a string describing the declaration of the given <see cref="INode"/>.
/// </summary>
public static class DeclarationString
{
    /// <summary>
    /// Gets the declaration from the given <see cref="INamespaceChild"/>.
    /// </summary>
    /// <param name="child">The <see cref="INamespaceChild"/> to get the declaration from.</param>
    /// <exception cref="NotImplementedException">
    /// When the given <see cref="INamespaceChild"/> is not yet implemented.
    /// </exception>
    public static string From(INamespaceChild child) =>
        child switch
        {
            PermutationVariable variable => From(variable),
            Type type => From(type),
            Interface @interface => From(@interface),
            ShaderFragment fragment => From(fragment),
            Shader shader => From(shader),
            Material material => From(material),
            _ => throw new NotImplementedException()
        };

    /// <summary>
    /// Gets the declaration from the given <see cref="IShaderMember"/>.
    /// </summary>
    /// <param name="member">The <see cref="IShaderMember"/> to get the declaration from.</param>
    /// <exception cref="NotImplementedException">
    /// When the given <see cref="IShaderMember"/> is not yet implemented.
    /// </exception>
    public static string From(IShaderMember member) =>
        member switch
        {
            GlobalVariable variable => From(variable),
            StructuredBuffer buffer => From(buffer),
            TypedBuffer buffer => From(buffer),
            Type type => From(type),
            ShaderFunction function => From(function),
            Stream stream => From(stream),
            _ => throw new NotImplementedException()
        };

    /// <summary>
    /// Gets the declaration from the given <see cref="ILiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="ILiteral"/> to get the declaration from.</param>
    /// <exception cref="NotImplementedException">
    /// When the given <see cref="ILiteral"/> is not yet implemented.
    /// </exception>
    public static string From(ILiteral literal) =>
        literal switch
        {
            FloatLiteral floatLiteral => From(floatLiteral),
            IntegerLiteral intLiteral => From(intLiteral),
            UnsignedIntegerLiteral unsignedIntegerLiteral => From(unsignedIntegerLiteral),
            DoubleLiteral doubleLiteral => From(doubleLiteral),
            BoolLiteral boolLiteral => From(boolLiteral),
            StringLiteral stringLiteral => From(stringLiteral),
            _ => throw new NotImplementedException()
        };

    /// <summary>
    /// Gets the declaration from the given <see cref="IDataType"/>.
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string From(IDataType dataType) =>
        dataType switch
        {
            BuiltInDataType type => From(type),
            PrimitiveDataType type => From(type),
            UserDefinedDataType type => From(type),
            UnknownDataType type => From(type),
            _ => throw new NotImplementedException()
        };

    public static string From(IExpression expression) =>
        expression switch
        {
            IConstantExpression constantExpression => From(constantExpression),
            _ => throw new NotImplementedException()
        };

    public static string From(IConstantExpression expression) =>
        expression switch
        {
            IPrimitiveExpression primitiveExpression => From(primitiveExpression),
            _ => throw new NotImplementedException()
        };

    public static string From(IPrimitiveExpression expression) =>
        expression switch
        {
            ILiteral literal => From(literal),
            _ => throw new NotImplementedException()
        };

    /// <summary>
    /// Gets the declaration of the given <see cref="PermutationVariable"/>.
    /// </summary>
    /// <param name="variable">The <see cref="PermutationVariable"/> to get the declaration from.</param>
    public static string From(PermutationVariable variable) =>
        $"permutation {variable.Type switch
        {
            PermutationVariableType.Bool => "bool",
            PermutationVariableType.Enum => "enum",
            _ => "int"
        }} {From(variable.Name)} = {From(variable.Initializer)};";

    /// <summary>
    /// Gets the declaration of the given <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the declaration from.</param>
    public static string From(Type type) =>
        $"type {From(type.Name)} as {(type.Kind == TypeKind.Struct ? "struct" : "enum")}";

    /// <summary>
    /// Gets the declaration of the given <see cref="TypeProperty"/>.
    /// </summary>
    /// <param name="property">The <see cref="TypeProperty"/> to get the declaration from.</param>
    public static string From(TypeProperty property)
    {
        StringBuilder sb = new();
        sb.Append($"{From(property.Type)} {From(property.Name)}");

        if (property.Initializer != null)
            sb.Append($" = {From(property.Initializer)}");

        sb.Append(';');

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="TypeFunction"/>.
    /// </summary>
    /// <param name="variable">The <see cref="TypeFunction"/> to get the declaration from.</param>
    public static string From(TypeFunction variable) => From(variable.Function);

    public static string From(Function function) =>
        $"{From(function.Head.ReturnType)} {From(function.Name)}({string.Join(", ", function.Head.Signature.Arguments.Select(From))})";

    public static string From(FunctionArgument parameter)
    {
        StringBuilder sb = new();

        if (parameter.Flow != DataFlow.Unspecified)
            sb.Append($"{parameter.Flow switch {
                DataFlow.In => "in",
                DataFlow.Out => "out",
                DataFlow.InOut => "inout",
                DataFlow.Const => "const",
                _ => string.Empty
            }} ");

        sb.Append($"{From(parameter.Type)} {From(parameter.Name)}");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="Interface"/>.
    /// </summary>
    /// <param name="variable">The <see cref="Interface"/> to get the declaration from.</param>
    public static string From(Interface variable)
    {
        StringBuilder sb = new($"interface {From(variable.Name)}");

        if (variable.ExtendedInterfaces.Count > 0)
        {
            sb.Append(" extends");

            foreach (NamespacedReference extends in variable.ExtendedInterfaces)
                sb.Append($" {extends.Name},");
        }

        return sb.ToString().TrimEnd(',');
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="ShaderFragment"/>.
    /// </summary>
    /// <param name="fragment">The <see cref="ShaderFragment"/> to get the declaration from.</param>
    public static string From(ShaderFragment fragment)
    {
        StringBuilder sb = new($"fragment {From(fragment.Name)}");

        if (fragment.ExtendedShaderFragment != NamespacedReference.Null)
            sb.Append($" extends {fragment.ExtendedShaderFragment.Name}");

        if (fragment.ExtendedInterfaces.Count > 0)
        {
            sb.Append(" implements");

            foreach (NamespacedReference extends in fragment.ExtendedInterfaces)
                sb.Append($" {extends.Name},");
        }

        return sb.ToString().TrimEnd(',');
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="Shader"/>.
    /// </summary>
    /// <param name="shader">The <see cref="Shader"/> to get the declaration from.</param>
    public static string From(Shader shader)
    {
        StringBuilder sb = new();

        if (shader.IsAbstract)
            sb.Append("abstract ");

        if (shader.Stage != ShaderStage.Unspecified)
            sb.Append($"{shader.Stage switch {
                ShaderStage.Vertex => "vertex",
                ShaderStage.Hull => "hull",
                ShaderStage.Domain => "domain",
                ShaderStage.Geometry => "geometry",
                ShaderStage.Pixel => "pixel",
                ShaderStage.Compute => "compute",
                _ => string.Empty
            }} ");

        sb.Append($"shader {From(shader.Name)}");

        if (shader.ExtendedShader != NamespacedReference.Null)
            sb.Append($" extends {shader.ExtendedShader.Name}");

        if (shader.Interfaces.Count > 0)
        {
            sb.Append(" implements");

            foreach (NamespacedReference implements in shader.Interfaces)
                sb.Append($" {implements.Name},");
        }

        return sb.ToString().TrimEnd(',');
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="Material"/>.
    /// </summary>
    /// <param name="material">The <see cref="Material"/> to get the declaration from.</param>
    public static string From(Material material)
    {
        StringBuilder sb = new();

        if (material.IsAbstract)
            sb.Append("abstract ");

        sb.Append($"material {From(material.Name)}");

        if (material.ExtendedMaterial != NamespacedReference.Null)
            sb.Append($" extends {material.ExtendedMaterial.Name}");

        return sb.ToString().TrimEnd(',');
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="GlobalVariable"/>
    /// </summary>
    /// <param name="variable">The <see cref="GlobalVariable"/> to get the declaration from.</param>
    public static string From(GlobalVariable variable)
    {
        StringBuilder sb = new();

        if (variable.IsStatic)
            sb.Append("static ");

        sb.Append($"const {From(variable.Type)} {From(variable.Name)} = {From(variable.Initializer)};");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="StructuredBuffer"/>.
    /// </summary>
    /// <param name="buffer">The <see cref="StructuredBuffer"/> to get the declaration from.</param>
    public static string From(StructuredBuffer buffer)
    {
        StringBuilder sb = new();

        if (buffer.Storage != BufferStorage.Undefined)
            sb.Append($"{buffer.Storage switch {
                BufferStorage.Coherent => "coherent",
                _ => string.Empty
            }} ");

        sb.Append($"{buffer.Access switch {
            BufferAccess.Constant => "const",
            BufferAccess.ReadOnly => "readonly",
            BufferAccess.WriteOnly => "writeonly",
            BufferAccess.ReadWrite => "readwrite",
            _ => string.Empty
        }} buffer {From(buffer.Name)}");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="TypedBuffer"/>.
    /// </summary>
    /// <param name="buffer">The <see cref="TypedBuffer"/> to get the declaration from.</param>
    public static string From(TypedBuffer buffer)
    {
        StringBuilder sb = new();

        if (buffer.Storage != BufferStorage.Undefined)
            sb.Append($"{buffer.Storage switch {
                BufferStorage.Coherent => "coherent",
                _ => string.Empty
            }} ");

        sb.Append($"{buffer.Access switch {
            BufferAccess.Constant => "const",
            BufferAccess.ReadOnly => "readonly",
            BufferAccess.WriteOnly => "writeonly",
            BufferAccess.ReadWrite => "readwrite",
            _ => string.Empty
        }} buffer<{From(buffer.DataType)}> {From(buffer.Name)};");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="ShaderFunction"/>.
    /// </summary>
    /// <param name="function">The <see cref="ShaderFunction"/> to get the declaration from.</param>
    public static string From(ShaderFunction function)
    {
        StringBuilder sb = new();

        if (function.IsConstructor)
        {
            sb.Append($"{From(function.Name)}()");
            return sb.ToString();
        }

        if (function.IsOverride)
            sb.Append("override ");

        sb.Append($"{From(function.Function)}");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to get the declaration from.</param>
    public static string From(Stream stream) => "stream";

    /// <summary>
    /// Gets the declaration of the given <see cref="FloatLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="FloatLiteral"/> to get the declaration from.</param>
    public static string From(FloatLiteral literal)
    {
        StringBuilder output = new();

        output.Append(literal.Value);

        if (Math.Abs((int)literal.Value - literal.Value) == 0)
            output.Append(".0");

        output.Append('f');

        return output.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="IntegerLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="IntegerLiteral"/> to get the declaration from.</param>
    public static string From(IntegerLiteral literal)
    {
        StringBuilder output = new();

        if (literal.IsHexConstant)
            output.Append("0x");

        if (literal.IsOctalConstant)
            output.Append('0');

        output.Append
        (
            Convert.ToString(literal.Value, literal.IsHexConstant ? 16 : literal.IsOctalConstant ? 8 : 10).ToUpper()
        );

        return output.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="UnsignedIntegerLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="UnsignedIntegerLiteral"/> to get the declaration from.</param>
    public static string From(UnsignedIntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Gets the declaration of the given <see cref="DoubleLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="DoubleLiteral"/> to get the declaration from.</param>
    public static string From(DoubleLiteral literal)
    {
        StringBuilder output = new();

        output.Append(literal.Value);

        if (Math.Abs((int)literal.Value - literal.Value) == 0)
            output.Append(".0");

        return output.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="BoolLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="BoolLiteral"/> to get the declaration from.</param>
    public static string From(BoolLiteral literal) => literal.Value ? "true" : "false";

    /// <summary>
    /// Gets the declaration of the given <see cref="StringLiteral"/>.
    /// </summary>
    /// <param name="literal">The <see cref="StringLiteral"/> to get the declaration from.</param>
    public static string From(StringLiteral literal) => $"\"{literal.Value}\"";

    /// <summary>
    /// Gets the declaration of the given <see cref="BuiltInDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="BuiltInDataType"/> to get the declaration from.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the given <see cref="BuiltInDataType"/> is an unknown or invalid built-in data type.
    /// </exception>
    public static string From(BuiltInDataType type)
    {
        StringBuilder sb = new();

        switch (type.Type)
        {
            case BuiltInDataTypeKind.Vector2b:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR2B).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector2f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR2F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector2d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR2D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector2i:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR2I).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector2ui:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR2UI).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector3b:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR3B).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector3f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR3F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector3d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR3D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector3i:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR3I).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector3ui:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR3UI).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector4b:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR4B).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector4f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR4F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector4d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR4D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector4i:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR4I).Trim('\''));
                break;
            case BuiltInDataTypeKind.Vector4ui:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VECTOR4UI).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2x3f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X3F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2x3d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X3D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2x4f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X4F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix2x4d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX2X4D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3x2f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X2F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3x2d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X2D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3x4f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X4F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix3x4d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX3X4D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4x2f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X2F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4x2d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X2D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4x3f:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X3F).Trim('\''));
                break;
            case BuiltInDataTypeKind.Matrix4x3d:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_MATRIX4X3D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Color3:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_COLOR3).Trim('\''));
                break;
            case BuiltInDataTypeKind.Color4:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_COLOR4).Trim('\''));
                break;
            case BuiltInDataTypeKind.Sampler:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_SAMPLER).Trim('\''));
                break;
            case BuiltInDataTypeKind.Texture1D:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_TEXTURE1D).Trim('\''));
                break;
            case BuiltInDataTypeKind.ArrayTexture1D:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_TEXTURE1DARRAY).Trim('\''));
                break;
            case BuiltInDataTypeKind.Texture2D:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_TEXTURE2D).Trim('\''));
                break;
            case BuiltInDataTypeKind.ArrayTexture2D:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_TEXTURE2DARRAY).Trim('\''));
                break;
            case BuiltInDataTypeKind.Texture3D:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_TEXTURE3D).Trim('\''));
                break;
            case BuiltInDataTypeKind.Cubemap:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_CUBEMAP).Trim('\''));
                break;
            case BuiltInDataTypeKind.ArrayCubemap:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_CUBEMAPARRAY).Trim('\''));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type.Type, "Unknown built-in data type.");
        }

        if (type.IsArray)
        {
            sb.Append('[');
            if (type.ArraySize != null)
                sb.Append(type.ArraySize);
            sb.Append(']');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="PrimitiveDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="PrimitiveDataType"/> to get the declaration from.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the given <see cref="PrimitiveDataType"/> is an unknown or invalid primitive data type.
    /// </exception>
    public static string From(PrimitiveDataType type)
    {
        StringBuilder sb = new();

        switch (type.Type)
        {
            case PrimitiveDataTypeKind.Void:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_VOID).Trim('\''));
                break;
            case PrimitiveDataTypeKind.Boolean:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_BOOL).Trim('\''));
                break;
            case PrimitiveDataTypeKind.Integer:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_INT).Trim('\''));
                break;
            case PrimitiveDataTypeKind.UnsignedInteger:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_UINT).Trim('\''));
                break;
            case PrimitiveDataTypeKind.Float:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_FLOAT).Trim('\''));
                break;
            case PrimitiveDataTypeKind.String:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_STRING).Trim('\''));
                break;
            case PrimitiveDataTypeKind.Double:
                sb.Append(DefaultVocabulary.GetLiteralName(TYPE_DOUBLE).Trim('\''));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type.Type, "Unknown primitive data type.");
        }

        if (type.IsArray)
        {
            sb.Append('[');
            if (type.ArraySize != null)
                sb.Append(type.ArraySize);
            sb.Append(']');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="UserDefinedDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="UserDefinedDataType"/> to get the declaration from.</param>
    public static string From(UserDefinedDataType type)
    {
        StringBuilder sb = new();

        sb.Append(type.Type.Name);

        if (type.IsArray)
        {
            sb.Append('[');
            if (type.ArraySize != null)
                sb.Append(type.ArraySize);
            sb.Append(']');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="UnknownDataType"/>.
    /// </summary>
    /// <param name="type">The <see cref="UnknownDataType"/> to get the declaration from.</param>
    public static string From(UnknownDataType type) => "unknown";

    /// <summary>
    /// Gets the declaration of the given <see cref="Identifier"/>.
    /// </summary>
    /// <param name="identifier">The <see cref="Identifier"/> to get the declaration from.</param>
    public static string From(Identifier identifier) => identifier.Value;

    /// <summary>
    /// Gets the declaration of the given <see cref="VariableDeclarationStatement"/>.
    /// </summary>
    /// <param name="statement">The <see cref="VariableDeclarationStatement"/> to get the declaration from.</param>
    public static string From(VariableDeclarationStatement statement) =>
        $"{From(statement.Type)} {From(statement.Name)}";

    /// <summary>
    /// Gets the declaration of the given <see cref="StreamProperty"/>.
    /// </summary>
    /// <param name="property">The <see cref="StreamProperty"/> to get the declaration from.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the given <see cref="StreamProperty"/> has an unknown or invalid stream property data flow.
    /// </exception>
    public static string From(StreamProperty property)
        => $"{property.DataFlow switch {
            StreamPropertyFlow.Input => "input",
            StreamPropertyFlow.Output => "output",
            StreamPropertyFlow.Transient => "transient",
            _ => throw new ArgumentOutOfRangeException(nameof(property.DataFlow), property.DataFlow, "Invalid stream property data flow.")
        }} {From(property.Type)} {From(property.Name)}";

    /// <summary>
    /// Gets the declaration string for the given <see cref="Annotation"/>.
    /// </summary>
    /// <param name="annotation">The <see cref="Annotation"/> to get the declaration from.</param>
    public static string From(Annotation annotation) => $"@{From(annotation.Identifier)}";
}
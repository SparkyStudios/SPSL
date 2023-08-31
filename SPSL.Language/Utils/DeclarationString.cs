using System.Text;
using SPSL.Language.AST;
using SPSL.Language.Core;
using Type = SPSL.Language.AST.Type;

namespace SPSL.Language.Utils;

/// <summary>
/// Gets a string describing the declaration of the given <see cref="INode"/>.
/// </summary>
public static class DeclarationString
{
    /// <summary>
    /// Gets the declaration of the given <see cref="INamespaceChild"/>.
    /// </summary>
    /// <param name="child">The <see cref="INamespaceChild"/> to get the declaration from.</param>
    /// <exception cref="NotImplementedException">
    /// When the given <see cref="INamespaceChild"/> is not yet implemented.
    /// </exception>
    public static string From(INamespaceChild child)
    {
        return child switch
        {
            PermutationVariable variable => From(variable),
            Type type => From(type),
            Interface @interface => From(@interface),
            ShaderFragment fragment => From(fragment),
            Shader shader => From(shader),
            Material material => From(material),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="IShaderMember"/>.
    /// </summary>
    /// <param name="member">The <see cref="IShaderMember"/> to get the declaration from.</param>
    /// <exception cref="NotImplementedException">
    /// When the given <see cref="IShaderMember"/> is not yet implemented.
    /// </exception>
    public static string From(IShaderMember member)
    {
        return member switch
        {
            GlobalVariable variable => From(variable),
            StructuredBuffer buffer => From(buffer),
            TypedBuffer buffer => From(buffer),
            Type type => From(type),
            ShaderFunction function => From(function),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="PermutationVariable"/>.
    /// </summary>
    /// <param name="variable">The <see cref="PermutationVariable"/> to get the declaration from.</param>
    public static string From(PermutationVariable variable) =>
        $"permutation {variable.Type.GetTypeName()} {variable.Name} = {variable.Initializer};";

    /// <summary>
    /// Gets the declaration of the given <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the declaration from.</param>
    public static string From(Type type) =>
        $"type {type.Name.Value} as {(type.Kind == TypeKind.Struct ? "struct" : "enum")}";

    /// <summary>
    /// Gets the declaration of the given <see cref="TypeProperty"/>.
    /// </summary>
    /// <param name="property">The <see cref="TypeProperty"/> to get the declaration from.</param>
    public static string From(TypeProperty property)
    {
        StringBuilder sb = new();
        sb.Append($"{property.Type} {property.Name}");

        if (property.Initializer != null)
            sb.Append($" = {property.Initializer}");

        sb.Append(';');

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="TypeFunction"/>.
    /// </summary>
    /// <param name="variable">The <see cref="TypeFunction"/> to get the declaration from.</param>
    public static string From(TypeFunction variable)
    {
        return From(variable.Function);
    }

    public static string From(Function function) =>
        $"{function.Head.ReturnType} {function.Name}({string.Join(", ", function.Head.Signature.Parameters.Select(From))})";

    public static string From(FunctionArgument parameter)
    {
        StringBuilder sb = new();

        if (parameter.Flow != DataFlow.Unspecified)
            sb.Append($"{parameter.Flow.ToString().ToLower()} ");

        sb.Append($"{parameter.Type} {parameter.Name}");

        return sb.ToString();
    }

    /// <summary>
    /// Gets the declaration of the given <see cref="Interface"/>.
    /// </summary>
    /// <param name="variable">The <see cref="Interface"/> to get the declaration from.</param>
    public static string From(Interface variable)
    {
        StringBuilder sb = new($"interface {variable.Name.Value}");

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
        StringBuilder sb = new($"fragment {fragment.Name.Value}");

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
            sb.Append($"{shader.Stage.ToString().ToLower()} ");

        sb.Append($"shader {shader.Name.Value}");

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
    /// Gets the declaration of the given <see cref="GlobalVariable"/>
    /// </summary>
    /// <param name="variable">The <see cref="GlobalVariable"/> to get the declaration from.</param>
    public static string From(GlobalVariable variable)
    {
        StringBuilder sb = new();

        if (variable.IsStatic)
            sb.Append("static ");

        sb.Append($"const {variable.Type} {variable.Name.Value} = {variable.Initializer};");

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
            sb.Append($"{buffer.Storage.ToString().ToLower()} ");

        sb.Append($"{buffer.Access.ToString().ToLower()} buffer {buffer.Name.Value}");

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
            sb.Append($"{buffer.Storage.ToString().ToLower()} ");

        sb.Append($"{buffer.Access.ToString().ToLower()} buffer<{buffer.DataType}> {buffer.Name.Value};");

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
            sb.Append($"{function.Name}()");
            return sb.ToString();
        }

        if (function.IsOverride)
            sb.Append("override ");

        sb.Append($"{From(function.Function)}");

        return sb.ToString();
    }
}
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

    public static string From(Function function)
    {
        return
            $"{function.Head.ReturnType} {function.Name}({string.Join(", ", function.Head.Signature.Parameters.Select(From))})";
    }

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
    /// <exception cref="NotImplementedException"></exception>
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
}
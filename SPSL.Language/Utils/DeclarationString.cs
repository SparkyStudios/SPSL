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
    /// Gets the declaration of the given <see cref="PermutationVariable"/>
    /// </summary>
    /// <param name="variable">The <see cref="PermutationVariable"/> to get the declaration from.</param>
    public static string From(PermutationVariable variable) =>
        $"permutation {variable.Type.GetTypeName()} {variable.Name} = {variable.Initializer};";

    /// <summary>
    /// Gets the declaration of the given <see cref="Type"/>
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the declaration from.</param>
    public static string From(Type type) =>
        $"type {type.Name.Value} as {(type.Kind == TypeKind.Struct ? "struct" : "enum")}";

    public static string From(TypeProperty property)
    {
        StringBuilder sb = new();
        sb.Append($"{property.Type} {property.Name}");

        if (property.Initializer != null)
            sb.Append($" = {property.Initializer}");

        sb.Append(';');

        return sb.ToString();
    }
}
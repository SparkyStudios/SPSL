namespace SPSL.Language.AST;

/// <summary>
/// Represents any node that can be a direct child of a <see cref="Namespace"/>.
/// </summary>
public interface INamespaceChild : IDocumented, INode
{
    /// <summary>
    /// The <see cref="Namespace"/> in which this child exists.
    /// </summary>
    Namespace? ParentNamespace { get; set; }

    /// <summary>
    /// The name of this namespace child.
    /// </summary>
    Identifier Name { get; set; }
}

public static class NamespaceChildExtensions
{
    public static string GetFullName(this INamespaceChild child) =>
        $"{child.ParentNamespace?.FullName}{Namespace.Separator}{child.Name.Value}";

    public static NamespacedReference GetReference(this INamespaceChild child) =>
        new(GetFullName(child).Split(Namespace.Separator).Select(x => new Identifier { Value = x }).ToArray());
}
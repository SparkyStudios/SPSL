namespace SPSL.Language.AST;

public interface INamespaceChild : INode
{
    /// <summary>
    /// The <see cref="Language.AST.Namespace"/> in which this child exists.
    /// </summary>
    Namespace? Parent { get; set; }

    /// <summary>
    /// The name of this namespace child.
    /// </summary>
    string Name { get; set; }
}

public static class INamespaceChildExtensions
{
    public static string GetFullName(this INamespaceChild child) => $"{child.Parent?.FullName}::{child.Name}";

    public static NamespacedReference GetReference(this INamespaceChild child) => new(GetFullName(child));
}
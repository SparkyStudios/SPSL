namespace SPSL.Language.AST;

public interface INamespaceChild
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
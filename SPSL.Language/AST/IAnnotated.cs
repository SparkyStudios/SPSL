using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a node which can take annotations.
/// </summary>
public interface IAnnotated
{
    /// <summary>
    /// The list of annotations applied to this node.
    /// </summary>
    public OrderedSet<Annotation> Annotations { get; }
}
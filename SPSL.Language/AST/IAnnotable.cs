using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an element which can take annotations.
/// </summary>
public interface IAnnotable
{
    public OrderedSet<Annotation> Annotations { get; }
}
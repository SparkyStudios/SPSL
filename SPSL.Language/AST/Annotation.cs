using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public record Annotation
{
    #region Properties

    public string Name { get; init; } = null!;
    public OrderedSet<IExpression> Arguments { get; init; } = new();

    #endregion
}
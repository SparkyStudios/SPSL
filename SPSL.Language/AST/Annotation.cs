using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Annotation : INode
{
    #region Properties

    public string Name { get; init; } = null!;

    public OrderedSet<IExpression> Arguments { get; init; } = new();

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Annotation : INode
{
    #region Properties

    public string Name { get; init; } = null!;

    public OrderedSet<IExpression> Arguments { get; init; } = new();

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class NewInstanceExpression : IExpression
{
    #region Properties

    public IDataType Type { get; set; }

    public OrderedSet<InvocationParameter> Parameters { get; set; }

    #endregion

    #region Constructors

    public NewInstanceExpression(IDataType type, IEnumerable<InvocationParameter>? parameters = null)
    {
        Type = type;
        Parameters = new(parameters);
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
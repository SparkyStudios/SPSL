using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class NewInstanceExpression : IExpression
{
    #region Properties

    public IDataType Type { get; }

    public OrderedSet<InvocationParameter> Parameters { get; }

    #endregion

    #region Constructors

    public NewInstanceExpression(IDataType type, IEnumerable<InvocationParameter>? parameters = null)
    {
        Type = type;
        Parameters = new(parameters);

        Type.Parent = this;
        foreach (InvocationParameter parameter in Parameters)
            parameter.Parent = this;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Type.ResolveNode(source, offset) ??
               Parameters.FirstOrDefault(p => p.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
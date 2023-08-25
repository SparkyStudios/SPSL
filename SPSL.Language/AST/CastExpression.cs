namespace SPSL.Language.AST;

/// <summary>
/// Represents a value cast expression.
/// </summary>
public class CastExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The type to cast to.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The expression to cast.
    /// </summary>
    public IExpression Expression { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="CastExpression"/>.
    /// </summary>
    /// <param name="type">The type to cast to.</param>
    /// <param name="expression">The expression to cast into the given <paramref name="type"/>.</param>
    public CastExpression(IDataType type, IExpression expression)
    {
        type.Parent = this;
        expression.Parent = this;
        
        Type = type;
        Expression = expression;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Type.ResolveNode(source, offset) ?? Expression.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
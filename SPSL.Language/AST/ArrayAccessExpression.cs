namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL array access expression.
/// </summary>
/// <example>
/// <code>anArray[anIndex]</code>
/// </example>x
public class ArrayAccessExpression : IAssignableExpression
{
    #region Properties

    /// <summary>
    /// The expression which returns an array.
    /// </summary>
    public IExpression Expression { get; }

    /// <summary>
    /// The expression returning the array index.
    /// </summary>
    public IExpression ArrayIndex { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="ArrayAccessExpression"/>
    /// </summary>
    /// <param name="expression">The expression returning the array.</param>
    /// <param name="arrayIndex">The expression returning the array index.</param>
    public ArrayAccessExpression(IExpression expression, IExpression arrayIndex)
    {
        expression.Parent = this;
        arrayIndex.Parent = this;
        
        Expression = expression;
        ArrayIndex = arrayIndex;
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
        return Expression.ResolveNode(source, offset) ?? ArrayIndex.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
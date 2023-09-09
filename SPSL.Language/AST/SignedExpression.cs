using SPSL.Language.Core;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an expression prefixed with a + or - sign
/// </summary>
public class SignedExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The operation sign.
    /// </summary>
    /// <value>
    /// + or -.
    /// </value> 
    public Op Sign { get; }

    /// <summary>
    /// The expression.
    /// </summary>
    public IExpression Expression { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="SignedExpression"/> class.
    /// </summary>
    /// <param name="sign">The operation sign.</param>
    /// <param name="expression">The expression.</param>
    public SignedExpression(Op sign, IExpression expression)
    {
        expression.Parent = this;

        Sign = sign;
        Expression = expression;
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
        return Expression.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
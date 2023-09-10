namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an user defined constant expression.
/// </summary>
public class UserDefinedConstantExpression : IConstantExpression
{
    #region Properties

    /// <summary>
    /// The reference to the constant value identifier.
    /// </summary>
    public NamespacedReference Identifier { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="UserDefinedConstantExpression"/>.
    /// </summary>
    /// <param name="identifier">The reference to the constant value identifier.</param>
    public UserDefinedConstantExpression(NamespacedReference identifier)
    {
        identifier.Parent = this;

        Identifier = identifier;
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
        return Identifier.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
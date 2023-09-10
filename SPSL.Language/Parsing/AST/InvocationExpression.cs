using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a method or function call.
/// </summary>
public class InvocationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The method/function name.
    /// </summary>
    public NamespacedReference Name { get; }

    /// <summary>
    /// The list of invocation parameters.
    /// </summary>
    public OrderedSet<InvocationParameter> Parameters { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="InvocationExpression"/>.
    /// </summary>
    /// <param name="name">The name of the method/function to call.</param>
    /// <param name="parameters">The list of parameters of this call.</param>
    public InvocationExpression(NamespacedReference name, IEnumerable<InvocationParameter> parameters)
    {
        Name = name;
        Parameters = new(parameters);

        Name.Parent = this;

        foreach (InvocationParameter parameter in Parameters)
            parameter.Parent = this;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvocationExpression"/>.
    /// </summary>
    /// <param name="name">The name of the method/function to call.</param>
    /// <param name="parameters">The list of parameters of this call.</param>
    public InvocationExpression(NamespacedReference name, params InvocationParameter[] parameters)
        : this(name, (IEnumerable<InvocationParameter>)parameters)
    {
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
        return Name.ResolveNode(source, offset) ??
               Parameters.FirstOrDefault(p => p.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
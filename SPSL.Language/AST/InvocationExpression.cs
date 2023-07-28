using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent a method or function call.
/// </summary>
public class InvocationExpression : IExpression
{
    #region Properties

    /// <summary>
    /// The method/function name.
    /// </summary>
    public NamespacedReference Name { get; set; }

    /// <summary>
    /// The list of invocation parameters.
    /// </summary>
    public OrderedSet<InvocationParameter> Parameters { get; set; }

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
        Parameters = new OrderedSet<InvocationParameter>(parameters);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="InvocationExpression"/>.
    /// </summary>
    /// <param name="name">The name of the method/function to call.</param>
    /// <param name="parameters">The list of parameters of this call.</param>
    public InvocationExpression(NamespacedReference name, params InvocationParameter[] parameters)
    {
        Name = name;
        Parameters = new OrderedSet<InvocationParameter>(parameters);
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an SPSL function.
/// </summary>
public class Function : IBlockChild, ISemanticallyEquatable, IEquatable<Function>
{
    #region Properties

    /// <summary>
    /// The function head. Contains the return type, the name, and the parameters.
    /// </summary>
    public FunctionHead Head { get; }

    /// <summary>
    /// The function body. An ordered list of statements.
    /// </summary>
    public StatementBlock Body { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Function"/> class.
    /// </summary>
    /// <param name="head">The function's head.</param>
    /// <param name="body">The function's body.</param>
    public Function(FunctionHead head, StatementBlock body)
    {
        head.Parent = this;
        body.Parent = this;

        Head = head;
        Body = body;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Function"/> class.
    /// </summary>
    /// <param name="head">The function's head.</param>
    /// <param name="statements">An ordered list of statements in the function's body.</param>
    public Function(FunctionHead head, params IStatement[] statements)
    {
        head.Parent = this;
        foreach (IStatement child in statements)
            child.Parent = this;

        Head = head;
        Body = new(statements)
        {
            Start = statements[0].Start,
            End = statements[^1].End
        };
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Function)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(Head, Body, Start, End, Source);
    }

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name"/>
    public Identifier Name => Head.Name;

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
        return Head.ResolveNode(source, offset) ?? Body.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode?)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not Function other) return false;

        // A function is semantically equivalent to another function if their function head are semantically equivalent.
        return Head.SemanticallyEquals(other.Head);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return Head.GetSemanticHashCode();
    }

    #endregion

    #region IEquatable<Function> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(Function? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Head.Equals(other.Head) && Body.Equals(other.Body) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
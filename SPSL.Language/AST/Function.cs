namespace SPSL.Language.AST;

public class Function : IBlockChild
{
    #region Properties

    public FunctionHead Head { get; }

    public StatementBlock Body { get; }

    #endregion

    #region Constructors

    public Function(FunctionHead head, StatementBlock body)
    {
        head.Parent = this;
        body.Parent = this;

        Head = head;
        Body = body;
    }

    public Function(FunctionHead head, params IStatement[] children)
    {
        head.Parent = this;
        foreach (var child in children)
            child.Parent = this;

        Head = head;
        Body = new(children)
        {
            Start = children[0].Start,
            End = children[^1].End
        };
    }

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name"/>
    public Identifier Name
    {
        get => Head.Name;
        set => Head.Name = value;
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
        return Head.ResolveNode(source, offset) ?? Body.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
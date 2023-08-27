namespace SPSL.Language.AST;

public class FunctionHead : IDocumented, INode, IEquatable<FunctionHead>
{
    public IDataType ReturnType { get; set; }

    public Identifier Name { get; set; }

    public FunctionSignature Signature { get; }

    public FunctionHead(IDataType returnType, Identifier name, FunctionSignature signature)
    {
        returnType.Parent = this;
        name.Parent = this;
        signature.Parent = this;
        
        ReturnType = returnType;
        Name = name;
        Signature = signature;
    }

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

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
        return ReturnType.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Signature.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region IEquatable<FunctionHead> Implementation

    public bool Equals(FunctionHead? other)
    {
        return other is not null && ReturnType.Equals(other.ReturnType) && Name.Value.Equals(other.Name.Value) &&
               Signature.Equals(other.Signature);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as FunctionHead);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ReturnType, Name, Signature);
    }

    #endregion
}
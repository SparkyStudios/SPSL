namespace SPSL.Language.AST;

public class FunctionHead : INode, IEquatable<FunctionHead>
{
    public IDataType ReturnType { get; set; }

    public string Name { get; set; }

    public FunctionSignature Signature { get; set; }

    public FunctionHead(IDataType returnType, string name, FunctionSignature signature)
    {
        ReturnType = returnType;
        Name = name;
        Signature = signature;
    }
    
    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion

    #region IEquatable<FunctionHead> Implementation

    public bool Equals(FunctionHead? other)
    {
        return other is not null && ReturnType.Equals(other.ReturnType) && Name.Equals(other.Name) &&
               Signature.Equals(other.Signature);
    }

    #endregion
}
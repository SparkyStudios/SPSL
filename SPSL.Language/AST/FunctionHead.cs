namespace SPSL.Language.AST;

public class FunctionHead : IEquatable<FunctionHead>
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

    #region IEquatable<FunctionHead> Implementation

    public bool Equals(FunctionHead? other)
    {
        return other is not null && ReturnType.Equals(other.ReturnType) && Name.Equals(other.Name) && Signature.Equals(other.Signature);
    }

    #endregion
}
namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL function signature with its name and its return type.
/// </summary>
public class FunctionHead : IDocumented, INode, ISemanticallyEquatable<FunctionHead>, IEquatable<FunctionHead>
{
    #region Properties

    /// <summary>
    /// The function's return type.
    /// </summary>
    public IDataType ReturnType { get; set;  }

    /// <summary>
    /// The function's name.
    /// </summary>
    public Identifier Name { get; set; }

    /// <summary>
    /// The function's signature.
    /// </summary>
    public FunctionSignature Signature { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionHead"/> class.
    /// </summary>
    /// <param name="returnType">The return type of the function.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="signature">The function's signature.</param>
    public FunctionHead(IDataType returnType, Identifier name, FunctionSignature signature)
    {
        returnType.Parent = this;
        name.Parent = this;
        signature.Parent = this;

        ReturnType = returnType;
        Name = name;
        Signature = signature;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Clones this instance by optionally providing new parameters.
    /// </summary>
    /// <param name="returnType">An optional new return type for the function.</param>
    /// <param name="name">An optional new name for the function.</param>
    /// <param name="signature">An optional new signature for the function.</param>
    /// <returns>
    /// A new <see cref="FunctionHead"/> object cloned from this instance. The new instance will have the same
    /// <see cref="INode"/> properties, such as <see cref="INode.Source"/>, <see cref="INode.Start"/>, and others.
    /// </returns>
    public FunctionHead Clone
    (
        IDataType? returnType = null,
        Identifier? name = null,
        FunctionSignature? signature = null
    ) => new(returnType ?? ReturnType, name ?? Name, signature ?? Signature);

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals(obj as FunctionHead);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(ReturnType, Name, Signature, Start, End, Source);
    }

    #endregion

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
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return ReturnType.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Signature.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable<FunctionHead> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(FunctionHead? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        // A function head is semantically equivalent to another function head if the return type, the name, and
        // the signature are all semantically equivalent.
        return ReturnType.SemanticallyEquals(other.ReturnType) && Name.SemanticallyEquals(other.Name) &&
               Signature.SemanticallyEquals(other.Signature);
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine
        (
            ReturnType.GetSemanticHashCode(),
            Name.GetSemanticHashCode(),
            Signature.GetSemanticHashCode()
        );
    }

    #endregion

    #region IEquatable<FunctionHead> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(FunctionHead? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return ReturnType.Equals(other.ReturnType) && Name.Equals(other.Name) &&
               Signature.Equals(other.Signature) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
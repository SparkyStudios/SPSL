using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an SPSL function signature.
/// </summary>
public class FunctionSignature : INode, ISemanticallyEquatable<FunctionSignature>, IEquatable<FunctionSignature>
{
    #region Properties

    /// <summary>
    /// Gets the list of <see cref="FunctionArgument"/>s for this function signature.
    /// </summary>
    public OrderedSet<FunctionArgument> Arguments { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionSignature"/> class.
    /// </summary>
    public FunctionSignature()
    {
        Arguments = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionSignature"/> class with the specified parameters.
    /// </summary>
    /// <param name="arguments">The list of <see cref="FunctionArgument"/>s to assign to this function signature.</param>
    public FunctionSignature(IEnumerable<FunctionArgument> arguments)
    {
        Arguments = new(arguments);

        foreach (FunctionArgument parameter in Arguments)
            parameter.Parent = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionSignature"/> class with the specified parameters.
    /// </summary>
    /// <param name="arguments">The list of <see cref="FunctionArgument"/>s to assign to this function signature.</param>
    public FunctionSignature(params FunctionArgument[] arguments)
    {
        foreach (FunctionArgument parameter in arguments)
            parameter.Parent = this;

        Arguments = new(arguments);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add the specified <see cref="FunctionArgument"/> to the end of the list of parameters.
    /// </summary>
    /// <param name="argument">The <see cref="FunctionArgument"/> to add.</param>
    public void AddParameter(FunctionArgument argument)
    {
        argument.Parent = this;

        Arguments.Add(argument);
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)"/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((FunctionSignature)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()"/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Arguments, Start, End, Source);
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
        return Arguments.FirstOrDefault(p => p.ResolveNode(source, offset) is not null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable<FunctionSignature> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(FunctionSignature? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Arguments.Count != other.Arguments.Count) return false;

        // A function signature is semantically equivalent to another function signature if all its parameters are semantically equivalent.
        for (uint i = 0, l = (uint)Arguments.Count; i < l; i++)
            if (Arguments[i].SemanticallyEquals(other.Arguments[i]) == false)
                return false;

        return true;
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return Arguments.GetSemanticHashCode<FunctionArgument>();
    }

    #endregion

    #region IEquatable<FunctionSignature> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(FunctionSignature? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Arguments.Count != other.Arguments.Count) return false;

        return Arguments.Equals(other.Arguments);
    }

    #endregion
}
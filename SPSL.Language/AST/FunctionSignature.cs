using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents an SPSL function signature.
/// </summary>
public class FunctionSignature : INode, IEquatable<FunctionSignature>
{
    #region Properties

    public OrderedSet<FunctionArgument> Parameters { get; }

    #endregion

    #region Constructors

    public FunctionSignature()
    {
        Parameters = new();
    }

    public FunctionSignature(IEnumerable<FunctionArgument> parameters)
    {
        Parameters = new(parameters);

        foreach (FunctionArgument parameter in Parameters)
            parameter.Parent = this;
    }

    public FunctionSignature(params FunctionArgument[] parameters)
    {
        foreach (FunctionArgument parameter in parameters)
            parameter.Parent = this;

        Parameters = new(parameters);
    }

    #endregion

    #region Methods

    public void AddParameter(FunctionArgument parameter)
    {
        parameter.Parent = this;
        
        Parameters.Add(parameter);
    }

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        return Equals(obj as FunctionSignature);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Parameters, Start, End, Source);
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
        return Parameters.FirstOrDefault(p => p.ResolveNode(source, offset) is not null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region IEquatable<FunctionSignature> Implementation

    public bool Equals(FunctionSignature? other)
    {
        if (other is null) return false;
        if (Parameters.Count != other.Parameters.Count) return false;

        // Slow array-like indexation, but needed for ordered comparison. Maybe a better way can be found
        for (uint i = 0, l = (uint)Parameters.Count; i < l; i++)
            if (Parameters[i].Equals(other.Parameters[i]) is false)
                return false;

        return true;
    }

    #endregion
}
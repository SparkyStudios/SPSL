using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL function signature.
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
    }

    public FunctionSignature(params FunctionArgument[] parameters)
    {
        Parameters = new(parameters);
    }

    #endregion

    #region Methods

    public void AddParameter(FunctionArgument parameter)
    {
        Parameters.Add(parameter);
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

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
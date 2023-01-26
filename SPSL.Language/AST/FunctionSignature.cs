using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL function signature.
/// </summary>
public class FunctionSignature : IEquatable<FunctionSignature>
{
    #region Properties

    public OrderedSet<FunctionArgument> Parameters { get; }

    #endregion

    #region Constructors

    public FunctionSignature()
    {
        Parameters = new OrderedSet<FunctionArgument>();
    }

    public FunctionSignature(IEnumerable<FunctionArgument> parameters)
    {
        Parameters = new OrderedSet<FunctionArgument>(parameters);
    }

    public FunctionSignature(params FunctionArgument[] parameters)
    {
        Parameters = new OrderedSet<FunctionArgument>(parameters);
    }

    #endregion

    #region Methods

    public void AddParameter(FunctionArgument parameter)
    {
        Parameters.Add(parameter);
    }

    #endregion

    #region IEquatable<FunctionSignature> Implementation

    public bool Equals(FunctionSignature? other)
    {
        if (other is null) return false;
        if (Parameters.Count != other.Parameters.Count) return false;

        // Slow array-like indexation, but needed for ordered comparison. Maybe a better way can be found
        for (uint i = 0, l = (uint)Parameters.Count; i < l; i++)
            if (Parameters[i].Equals(other.Parameters[i]) is false) return false;

        return true;
    }

    #endregion
}
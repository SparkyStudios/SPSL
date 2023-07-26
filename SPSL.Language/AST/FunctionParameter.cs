namespace SPSL.Language.AST;

/// <summary>
/// Represent an SPSL function argument.
/// </summary>
public class FunctionArgument : INode, IEquatable<FunctionArgument>
{
    #region Properties

    /// <summary>
    /// The flow of the data.
    /// </summary>
    public DataFlow Flow { get; set; }

    /// <summary>
    /// The data type.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The argument's name.
    /// </summary>
    public string Name { get; }

    #endregion

    #region Methods

    public FunctionArgument(DataFlow flow, IDataType type, string name)
    {
        Flow = flow;
        Type = type;
        Name = name;
    }

    #endregion

    #region INode Implementation

    public int Start { get; init; }

    public int End { get; init; }

    #endregion

    #region IEquatable<FunctionArgument> Implementation

    public bool Equals(FunctionArgument? other)
    {
        return other is not null && Flow.Equals(other.Flow) && Type.Equals(other.Type) && Name.Equals(other.Name);
    }

    #endregion
}
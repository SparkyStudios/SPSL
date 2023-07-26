using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent a property in a <see cref="Language.AST.Stream"/>.
/// </summary>
public class StreamProperty : IAnnotable, INode
{
    #region Properties

    /// <summary>
    /// The reference to the type definition of this member.
    /// </summary>
    public IDataType Type { get; set; }

    /// <summary>
    /// The member name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Specifies the flow direction of this member. <see cref="StreamPropertyFlow.Input"/> are
    /// used as vertex streams, <see cref="StreamPropertyFlow.Output"/> as edge streams.
    /// </summary>
    /// <value></value>
    public StreamPropertyFlow DataFlow { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="StreamProperty"/>.
    /// </summary>
    /// <param name="type">The member type.</param>
    /// <param name="name">The member name.</param>
    /// <param name="dataFlow">Specifies the flow direction of this member.</param>
    public StreamProperty(IDataType type, string name, StreamPropertyFlow dataFlow)
    {
        Type = type;
        Name = name;
        DataFlow = dataFlow;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
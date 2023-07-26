using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Stream : IShaderMember
{
    #region Properties

    /// <summary>
    /// The collection of members in this types.
    /// </summary>
    public OrderedSet<StreamProperty> Properties { get; }

    public string Name { get; set; }

    public IEnumerable<StreamProperty> Inputs => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Input);

    public IEnumerable<StreamProperty> Transients => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Transient);

    public IEnumerable<StreamProperty> Outputs => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Output);

    #endregion

    #region Constructors

    public Stream(string name, IEnumerable<StreamProperty> properties)
    {
        Name = name;
        Properties = new(properties);
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; }

    public int Start { get; init; }

    public int End { get; init; }

    #endregion
}
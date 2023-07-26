using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class StructuredBuffer : IShaderMember, IAnnotable
{
    #region Properties

    /// <summary>
    /// The collection of members in this types.
    /// </summary>
    public OrderedSet<TypeProperty> Properties { get; }

    public string Name { get; set; }

    public BufferStorage Storage { get; set; }

    public BufferAccess Access { get; set; }

    #endregion

    #region Constructors

    public StructuredBuffer(string name, IEnumerable<TypeProperty> properties)
    {
        Name = name;
        Properties = new(properties);
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
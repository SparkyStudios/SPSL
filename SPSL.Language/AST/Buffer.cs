using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Buffer : IShaderMember, IAnnotable
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

    public Buffer(string name, IEnumerable<TypeProperty> properties)
    {
        Name = name;
        Properties = new(properties);
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion
}

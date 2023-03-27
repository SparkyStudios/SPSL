using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represent a member of an <see cref="Language.AST.Type"/>.
/// </summary>
public class TypeProperty : IAnnotable
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

    public IConstantExpression? Initializer { get; set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="TypeProperty"/>.
    /// </summary>
    /// <param name="type">The member type.</param>
    /// <param name="name">The member name.</param>
    public TypeProperty(IDataType type, string name)
    {
        Type = type;
        Name = name;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion
}
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a member of an <see cref="AST.Type"/>.
/// </summary>
public class TypeProperty : IAnnotated, IDocumented, INode
{
    #region Properties

    /// <summary>
    /// The reference to the type definition of this member.
    /// </summary>
    public IDataType Type { get; }

    /// <summary>
    /// The member name.
    /// </summary>
    public Identifier Name { get; }

    public IConstantExpression? Initializer { get; init; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="TypeProperty"/>.
    /// </summary>
    /// <param name="type">The member type.</param>
    /// <param name="name">The member name.</param>
    public TypeProperty(IDataType type, Identifier name)
    {
        type.Parent = this;
        name.Parent = this;

        Type = type;
        Name = name;
    }

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations"/>
    public OrderedSet<Annotation> Annotations { get; init; } = new();

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

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
        return Type.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Initializer?.ResolveNode(source, offset) ??
            Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset);
    }

    #endregion
}
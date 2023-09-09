using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class StructuredBuffer : IShaderMember, IAnnotated
{
    #region Properties

    /// <summary>
    /// The collection of members in this types.
    /// </summary>
    public OrderedSet<TypeProperty> Properties { get; }

    public Identifier Name { get; set; }

    public BufferStorage Storage { get; init; }

    public BufferAccess Access { get; init; }

    #endregion

    #region Constructors

    public StructuredBuffer(Identifier name, IEnumerable<TypeProperty> properties)
    {
        name.Parent = this;

        Name = name;
        Properties = new(properties);

        foreach (TypeProperty property in Properties)
            property.Parent = this;
    }

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations"/>
    public OrderedSet<Annotation> Annotations { get; } = new();

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
        return Properties.FirstOrDefault(p => p.ResolveNode(source, offset) is not null)?.ResolveNode(source, offset) ??
               Name.ResolveNode(source, offset) ??
               Annotations.FirstOrDefault(p => p.ResolveNode(source, offset) is not null)
                   ?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a property in a <see cref="Language.AST.Stream"/>.
/// </summary>
public class StreamProperty : IAnnotated, INode
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

    /// <summary>
    /// Specifies the flow direction of this member. <see cref="StreamPropertyFlow.Input"/> are
    /// used as vertex streams, <see cref="StreamPropertyFlow.Output"/> as edge streams.
    /// </summary>
    public StreamPropertyFlow DataFlow { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initialize a new instance of <see cref="StreamProperty"/>.
    /// </summary>
    /// <param name="type">The member type.</param>
    /// <param name="name">The member name.</param>
    /// <param name="dataFlow">Specifies the flow direction of this member.</param>
    public StreamProperty(IDataType type, Identifier name, StreamPropertyFlow dataFlow)
    {
        type.Parent = this;
        name.Parent = this;

        Type = type;
        Name = name;
        DataFlow = dataFlow;
    }

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
        return Type.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
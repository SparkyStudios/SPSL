using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class Stream : IShaderMember
{
    #region Properties

    /// <summary>
    /// The collection of members in this types.
    /// </summary>
    public OrderedSet<StreamProperty> Properties { get; }

    public Identifier Name { get; set; }

    public IEnumerable<StreamProperty> Inputs => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Input);

    public IEnumerable<StreamProperty> Transients => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Transient);

    public IEnumerable<StreamProperty> Outputs => Properties.Where(p => p.DataFlow == StreamPropertyFlow.Output);

    #endregion

    #region Constructors

    public Stream(Identifier name, IEnumerable<StreamProperty> properties)
    {
        name.Parent = this;
        
        Name = name;
        Properties = new(properties);
        
        foreach (StreamProperty property in Properties)
            property.Parent = this;
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Properties.FirstOrDefault(p => p.ResolveNode(source, offset) is not null)?.ResolveNode(source, offset) ??
               Name.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
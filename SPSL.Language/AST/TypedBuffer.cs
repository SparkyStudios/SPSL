using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class TypedBuffer : IShaderMember, IAnnotated
{
    #region Properties

    public IDataType DataType { get; }

    public Identifier Name { get; set; }

    public BufferStorage Storage { get; init; }

    public BufferAccess Access { get; init; }

    #endregion

    #region Constructors

    public TypedBuffer(Identifier name, IDataType dataType)
    {
        name.Parent = this;
        dataType.Parent = this;

        Name = name;
        DataType = dataType;
    }

    #endregion

    #region IAnnotated Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = null!;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return DataType.ResolveNode(source, offset) ?? Name.ResolveNode(source, offset) ??
            Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
            (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion
}
using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class TypedBuffer : IShaderMember, IAnnotable
{
    #region Properties

    public IDataType DataType { get; set; }

    public string Name { get; set; }

    public BufferStorage Storage { get; set; }

    public BufferAccess Access { get; set; }

    #endregion

    #region Constructors

    public TypedBuffer(string name, IDataType dataType)
    {
        Name = name;
        DataType = dataType;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; } = new();

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
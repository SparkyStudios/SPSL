using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class ShaderFunction : IAnnotable, IShaderMember, IMaterialMember
{
    #region Properties

    public bool IsOverride { get; set; }

    public Function Function { get; set; }

    public bool IsConstructor { get; set; }

    #endregion

    #region Constructors

    public ShaderFunction(Function function)
    {
        Annotations = new();
        IsOverride = false;
        Function = function;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; }

    #endregion

    #region IBlockChild Implementation

    public string Name
    {
        get => ((IBlockChild)Function).Name;
        set => ((IBlockChild)Function).Name = value;
    }

    #endregion

    #region INode Implementation

    public string Source { get; set; } = null!;

    public int Start { get; set; } = -1;

    public int End { get; set; } = -1;

    #endregion
}
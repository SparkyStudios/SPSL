using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class ShaderFunction : IAnnotable, IBlockChild
{
    #region Properties

    public bool IsOverride { get; set; }

    public Function Function { get; set; }

    #endregion

    #region Construtors

    public ShaderFunction(Function function)
    {
        Annotations = new OrderedSet<Annotation>();
        IsOverride = false;
        Function = function;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; }

    #endregion
}
using SPSL.Language.Utils;

namespace SPSL.Language.AST;

/// <summary>
/// Represents a function inside a struct.
/// </summary>
public class TypeFunction : IAnnotable, IBlockChild
{
    #region Properties

    public Function Function { get; set; }

    #endregion

    #region Construtors

    public TypeFunction(Function function)
    {
        Annotations = new OrderedSet<Annotation>();
        Function = function;
    }

    #endregion

    #region IAnnotable Implementation

    public OrderedSet<Annotation> Annotations { get; }

    #endregion

    #region IBlockChild Implementation

    public string Name { get => ((IBlockChild)Function).Name; set => ((IBlockChild)Function).Name = value; }

    #endregion
}
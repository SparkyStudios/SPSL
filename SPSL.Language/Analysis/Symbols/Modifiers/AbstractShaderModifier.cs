namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class AbstractShaderModifier : ISymbolModifier
{
    #region Properties

    public bool IsAbstract { get; init; }

    #endregion

    #region Constructors

    public AbstractShaderModifier(bool isAbstract = false)
    {
        IsAbstract = isAbstract;
    }

    #endregion
}

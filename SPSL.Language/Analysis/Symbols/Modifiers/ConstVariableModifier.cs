namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class ConstVariableModifier : ISymbolModifier
{
    #region Properties

    public bool IsConst { get; }

    #endregion

    #region Constructors

    public ConstVariableModifier(bool isConst)
    {
        IsConst = isConst;
    }

    #endregion
}
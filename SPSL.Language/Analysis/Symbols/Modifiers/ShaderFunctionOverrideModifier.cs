namespace SPSL.Language.Analysis.Symbols.Modifiers;

public class ShaderFunctionOverrideModifier : ISymbolModifier
{
    #region Properties
    
    public bool IsOverride { get; }
    
    #endregion
    
    #region Constructors
    
    public ShaderFunctionOverrideModifier(bool isOverride)
    {
        IsOverride = isOverride;
    }
    
    #endregion
}
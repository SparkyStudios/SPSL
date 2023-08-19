namespace SPSL.Language.Symbols.Modifiers;

public class SymbolTypeModifier : ISymbolModifier
{
    #region Properties

    public string Name { get; }

    #endregion
    
    #region Constructors
    
    public SymbolTypeModifier(string name)
    {
        Name = name;
    }
    
    #endregion
}
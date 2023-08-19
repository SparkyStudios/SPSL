using SPSL.Language.Core;

namespace SPSL.Language.Symbols.Modifiers;

public class ParameterFlowModifier : ISymbolModifier
{
    #region Properties

    public DataFlow Flow { get; }

    #endregion
    
    #region Constructors
    
    public ParameterFlowModifier(DataFlow flow)
    {
        Flow = flow;
    }
    
    #endregion
}
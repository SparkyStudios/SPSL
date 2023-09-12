using SPSL.Language.Analysis.Symbols;

namespace SPSL.Language.Analysis.Common;

/// <summary>
/// Defines the state of a <see cref="Symbol"/>.
/// </summary>
public enum SymbolState
{
    /// <summary>
    /// The <see cref="Symbol"/> is in an invalid state. This usually means that an error
    /// occured during the analysis phase.  
    /// </summary>
    Invalid = 0,
    
    /// <summary>
    /// The <see cref="Symbol"/> is declared.
    /// </summary>
    Declared = 1,
    
    /// <summary>
    /// The <see cref="Symbol"/> is defined. This state should only be reached if the <see cref="Symbol"/>
    /// was previously declared.
    /// </summary>
    Defined = 2,
    
    /// <summary>
    /// The <see cref="Symbol"/> is used. This state should only be reached if the <see cref="Symbol"/>
    /// was previously defined.
    /// </summary>
    Used = 3,
}
namespace SPSL.Language.Symbols;

public enum SymbolType : byte
{
    Unknown = 0,

    Namespace,
    Permutation,
    Scope,
    Variable,
    Function,
    Type,
    Enum,
    Shader,
    Material,
    Fragment
}

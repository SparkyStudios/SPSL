namespace SPSL.Language.Symbols;

public enum SymbolType : byte
{
    Unknown = 0,
    Invalid = 1,

    Namespace,
    Permutation,
    Scope,
    Variable,
    Property,
    Function,
    Constructor,
    Parameter,
    Struct,
    Enum,
    Interface,
    Shader,
    Material,
    Fragment,
    Buffer,
}

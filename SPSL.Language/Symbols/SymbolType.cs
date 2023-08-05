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
    Parameter,
    Struct,
    Enum,
    Shader,
    Material,
    Fragment,
    Buffer,
}

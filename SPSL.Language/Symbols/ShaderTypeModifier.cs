namespace SPSL.Language.Symbols;

public class ShaderTypeModifier : ISymbolModifier
{
    public string Type { get; }

    public ShaderTypeModifier(string type)
    {
        Type = type;
    }
}
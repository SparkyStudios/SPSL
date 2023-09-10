namespace SPSL.Language.Parsing.Common;

[Flags]
public enum BufferStorage
{
    Undefined = 0,

    Coherent = 1 << 1,
}

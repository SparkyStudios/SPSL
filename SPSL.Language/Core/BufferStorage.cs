namespace SPSL.Language.Core;

[Flags]
public enum BufferStorage
{
    Undefined = 0,

    Coherent = 1 << 1,
}

using System;
using System.Diagnostics;

namespace SPSL.Language.Symbols;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct Reference : IEquatable<Reference>
{
    #region Properties

    public string Source { get; init; } = string.Empty;

    public int Start { get; init; } = -1;

    public int End { get; init; } = -1;

    #endregion

    #region Constructors

    public Reference()
    { }

    #endregion

    #region Methods

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    #endregion

    #region IEquatable<SymbolReference> Implementation

    public bool Equals(Reference other)
    {
        return Source.Equals(other.Source) && Start == other.Start &&
               End == other.End;
    }

    #endregion

    #region Overrides

    public override bool Equals(object? obj)
    {
        return obj is Reference reference && Equals(reference);
    }

    public override int GetHashCode()
    {
        return Source.GetHashCode() + Start + End;
    }

    public override string ToString()
    {
        return $"{Source}:{Start}:{End}";
    }

    #endregion

    #region Operators

    public static bool operator ==(Reference left, Reference right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Reference left, Reference right)
    {
        return !(left == right);
    }

    #endregion
}


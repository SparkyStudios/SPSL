namespace SPSL.Language.Analysis.Common;

/// <summary>
/// A single diagnostic result from the static analysis.
/// </summary>
public record Diagnostic
{
    /// <summary>
    /// The diagnostic severity.
    /// </summary>
    public Severity Severity { get; init; }

    /// <summary>
    /// The diagnostic code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The diagnostic message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// The path to the source file.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// The start index in the source file where the diagnostic was found.
    /// </summary>
    public int Start { get; init; } = -1;

    /// <summary>
    /// The end index in the source file where the diagnostic was found.
    /// </summary>
    public int End { get; init; } = -1;
}
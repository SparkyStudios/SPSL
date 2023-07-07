namespace SPSL.Translation.Common;

public record Prop
(
    string Type,
    string Name,
    string? Value = null,
    string? Semantic = null
);

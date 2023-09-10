namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents SPSL entities that can be documented through
/// doc comments.
/// </summary>
public interface IDocumented
{
    #region Properties

    /// <summary>
    /// The documentation comment for this entity.
    /// </summary>
    public string Documentation { get; }

    #endregion
}
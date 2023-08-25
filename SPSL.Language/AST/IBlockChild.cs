namespace SPSL.Language.AST;

/// <summary>
/// A child in a block of code.
/// </summary>
public interface IBlockChild : INode
{
    /// <summary>
    /// The name of the code block child.
    /// </summary>
    Identifier Name { get; set; }
}
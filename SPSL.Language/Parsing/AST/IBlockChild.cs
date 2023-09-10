namespace SPSL.Language.Parsing.AST;

/// <summary>
/// A child in a block of code.
/// </summary>
public interface IBlockChild : INode
{
    /// <summary>
    /// The name of the code block child.
    /// </summary>
    Identifier Name { get; }
}
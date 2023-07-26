namespace SPSL.Language.AST;

/// <summary>
/// Represent a node in the AST graph.
/// </summary>
public interface INode
{
    public int Start { get; init; }
    
    public int End { get; init; }
}
namespace SPSL.Language.AST;

/// <summary>
/// Represent a node in the AST graph.
/// </summary>
public interface INode
{
    public int Start { get; internal set; }
    
    public int End { get; internal set; }
    
    public string Source { get; internal set; }
}

public static class NodeExtensions
{
    /// <summary>
    /// Checks whether this node is a virtual node. Virtual nodes are not part of the document.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>
    /// <c>true</c> if this node is a virtual node; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsVirtualNode(this INode node) => node.Start == -1 || node.End == -1;
}
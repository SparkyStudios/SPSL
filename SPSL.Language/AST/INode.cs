namespace SPSL.Language.AST;

/// <summary>
/// Represent a node in the AST graph.
/// </summary>
public interface INode
{
    /// <summary>
    /// The start range of the node.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// The end range of the node.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// The path of the source file where this node is located.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// The parent <see cref="INode"/> of this node. If this is null,
    /// this node is the root of the <see cref="Ast"/>.
    /// </summary>
    public INode? Parent { get; internal set; }

    /// <summary>
    /// Resolves the node for the given <paramref name="source"/> and <paramref name="offset"/> in this current
    /// <see cref="INode"/> instance.
    /// </summary>
    /// <param name="source">The node source, may differ from this node source.</param>
    /// <param name="offset">The node offset in the source.</param>
    /// <returns>
    /// A possible child <see cref="INode"/> instance if this node contains the requested one, or this instance if
    /// this node match the request, or <see langword="null"/> if this node and no child match the request.
    /// </returns>
    public INode? ResolveNode(string source, int offset);
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

    /// <summary>
    /// Checks whether the given <paramref name="offset"/> is in the range of this node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="offset">The offset to check.</param>
    /// <returns>
    /// <c>true</c> if the given <paramref name="offset"/> is in the range of this node; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsOffsetInRange(this INode node, int offset) => offset >= node.Start && offset <= node.End;
}
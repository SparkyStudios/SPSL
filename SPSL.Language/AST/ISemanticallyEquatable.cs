namespace SPSL.Language.AST;

/// <summary>
/// Used on an <see cref="INode"/> implementation to allows to check for
/// semantic equality with another <see cref="INode"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the <see cref="INode"/> which implements semantic equality.
/// </typeparam>
public interface ISemanticallyEquatable<in T> where T : INode
{
    /// <summary>
    /// Checks if this <see cref="INode"/> is equal to another <see cref="INode"/>.
    /// </summary>
    /// <param name="other">The other <see cref="INode"/> to compare with.</param>
    /// <returns>
    /// <c>true</c> if this <see cref="INode"/> is semantically equal to the other <see cref="INode"/>,
    /// and <c>false</c> otherwise.
    /// </returns>
    public bool SemanticallyEquals(T? other);
    
    /// <summary>
    /// Gets an unique hash code to semantically identify this <see cref="INode"/>.
    /// </summary>
    public int GetSemanticHashCode();
}
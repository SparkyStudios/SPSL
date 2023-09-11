using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents a function inside a struct.
/// </summary>
public class TypeFunction : IAnnotated, IDocumented, IBlockChild, ISemanticallyEquatable,
    IEquatable<TypeFunction>
{
    #region Properties

    /// <summary>
    /// The wrapped function.
    /// </summary>
    public Function Function { get; }

    #endregion

    #region Construtors

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeFunction"/> class.
    /// </summary>
    /// <param name="function">The function to wrap in this <see cref="TypeFunction"/> node.</param>
    public TypeFunction(Function function)
    {
        function.Parent = this;

        Function = function;
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ShaderFunction)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine
        (
            Function,
            Start,
            End,
            Source
        );
    }

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations"/>
    public OrderedSet<Annotation> Annotations { get; init; } = new();

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation => Function.Head.Documentation;

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name"/>
    public Identifier Name => ((IBlockChild)Function).Name;

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; }

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode?)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not TypeFunction other) return false;

        // Two type functions are semantically equal if they have their inner function are also semantically equal.
        return Function.SemanticallyEquals(other.Function);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(Function.GetSemanticHashCode());
    }

    #endregion

    #region IEquatable<TypeFunction> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(TypeFunction? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Function.Equals(other.Function) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
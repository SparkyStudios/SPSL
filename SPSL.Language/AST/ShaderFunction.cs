using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public class ShaderFunction : IAnnotated, IShaderMember, IMaterialMember, ISemanticallyEquatable<ShaderFunction>,
    IEquatable<ShaderFunction>
{
    #region Properties

    /// <summary>
    /// Whether this function overrides a function with the same signature in the parent shaders.
    /// </summary>
    public bool IsOverride { get; init; }

    /// <summary>
    /// The wrapped function.
    /// </summary>
    public Function Function { get; }

    /// <summary>
    /// Whether this function is a shader constructor.
    /// </summary>
    public bool IsConstructor { get; init; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderFunction"/> class.
    /// </summary>
    /// <param name="function">The function to wrap in this <see cref="ShaderFunction"/> node.</param>
    public ShaderFunction(Function function)
    {
        function.Parent = this;

        Annotations = new();
        IsOverride = false;
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
            IsOverride,
            IsConstructor,
            Function,
            Start,
            End,
            Source
        );
    }

    #endregion

    #region IAnnotated Implementation

    /// <inheritdoc cref="IAnnotated.Annotations" />
    public OrderedSet<Annotation> Annotations { get; }

    #endregion

    #region IBlockChild Implementation

    /// <inheritdoc cref="IBlockChild.Name"/>
    public Identifier Name => ((IBlockChild)Function).Name;

    #endregion

    #region IDocumented Implementation

    /// <inheritdoc cref="IDocumented.Documentation"/>
    public string Documentation { get; init; } = string.Empty;

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
        return Function.ResolveNode(source, offset) ??
               Annotations.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable<ShaderFunction> Implementation

    /// <inheritdoc cref="ISemanticallyEquatable{T}.SemanticallyEquals(T?)"/>
    public bool SemanticallyEquals(ShaderFunction? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        // Two shader functions are semantically equal if they have their inner function are also semantically equal.
        return IsOverride == other.IsOverride && Function.SemanticallyEquals(other.Function);
    }

    /// <inheritdoc cref="ISemanticallyEquatable{T}.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return HashCode.Combine(IsOverride, Function.GetSemanticHashCode());
    }

    #endregion

    #region IEquatable<ShaderFunction> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T?)"/>
    public bool Equals(ShaderFunction? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return IsOverride == other.IsOverride && IsConstructor == other.IsConstructor &&
               Function.Equals(other.Function) && Start == other.Start && End == other.End &&
               Source == other.Source;
    }

    #endregion
}
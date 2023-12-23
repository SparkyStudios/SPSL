using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

/// <summary>
/// Represents an SPSL annotation.
/// </summary>
/// <example>
/// <code>@anAnnotation(val1, val2, ...)</code>
/// </example>
public class Annotation : INode, ISemanticallyEquatable, IEquatable<Annotation>
{
    #region Properties

    /// <summary>
    /// The name of the annotation.
    /// </summary>
    public Identifier Identifier { get; }

    /// <summary>
    /// The arguments of the annotation.
    /// </summary>
    public OrderedSet<IExpression> Arguments { get; }

    /// <summary>
    /// Checks whether the annotation is a 'semantic' annotation.
    /// 
    /// Semantic annotations are annotations used on stream properties to specify the semantics
    /// of input attributes.
    /// </summary>
    public bool IsSemantic => Identifier.Value is "semantic" or "position" or "texcoord" or "normal" or "tangent"
        or "bitangent" or "color" or "boneweights" or "boneindices";

    /// <summary>
    /// Checks whether the annotation is an 'entry' annotation.
    ///
    /// Entry annotations are used to mark a shader function as the entry point of that shader.
    /// </summary>
    public bool IsEntry => Identifier.Value is "entry";

    /// <summary>
    /// Checks whether the annotation is a 'precision' annotation.
    ///
    /// Precision annotations are used to specify the precision of an input attribute.
    /// </summary>
    public bool IsPrecision => Identifier.Value is "precision";
    
    /// <summary>
    /// Checks whether the annotation is a built-in SPSL annotation.
    /// </summary>
    public bool IsBuiltInAnnotation => IsSemantic || IsEntry || IsPrecision;
    
    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Annotation"/> class with the specified <paramref name="identifier"/>
    /// and <paramref name="arguments"/>.
    /// </summary>
    /// <param name="identifier">The name of the annotation.</param>
    /// <param name="arguments">The list of arguments.</param>
    public Annotation(Identifier identifier, IEnumerable<IExpression> arguments)
    {
        identifier.Parent = this;

        Identifier = identifier;
        Arguments = new(arguments);

        foreach (IExpression argument in Arguments)
            argument.Parent = this;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Annotation"/> class with the specified <paramref name="identifier"/>
    /// and <paramref name="arguments"/>.
    /// </summary>
    /// <param name="identifier">The name of the annotation.</param>
    /// <param name="arguments">The list of arguments.</param>
    public Annotation(Identifier identifier, params IExpression[] arguments)
        : this(identifier, (IEnumerable<IExpression>)arguments)
    {
    }

    #endregion

    #region Overrides

    /// <inheritdoc cref="Object.Equals(object?)" />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;

        return Equals((Annotation)obj);
    }

    /// <inheritdoc cref="Object.GetHashCode()" />
    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, Arguments, Start, End, Source);
    }

    #endregion

    #region INode Implementation

    /// <inheritdoc cref="INode.Start"/>
    public int Start { get; init; }

    /// <inheritdoc cref="INode.End"/>
    public int End { get; init; }

    /// <inheritdoc cref="INode.Source"/>
    public string Source { get; init; } = string.Empty;

    /// <inheritdoc cref="INode.Parent"/>
    public INode? Parent { get; set; } = null;

    /// <inheritdoc cref="INode.ResolveNode(string, int)"/>
    public INode? ResolveNode(string source, int offset)
    {
        return Identifier.ResolveNode(source, offset) ??
               Arguments.FirstOrDefault(a => a.ResolveNode(source, offset) != null)?.ResolveNode(source, offset) ??
               (Source == source && offset >= Start && offset <= End ? this as INode : null);
    }

    #endregion

    #region ISemanticallyEquatable Implementation

    /// <inheritdoc cref="ISemanticallyEquatable.SemanticallyEquals(INode?)"/>
    public bool SemanticallyEquals(INode? node)
    {
        if (ReferenceEquals(null, node)) return false;
        if (ReferenceEquals(this, node)) return true;
        if (node is not Annotation other) return false;

        return Identifier.SemanticallyEquals(other.Identifier);
    }

    /// <inheritdoc cref="ISemanticallyEquatable.GetSemanticHashCode()"/>
    public int GetSemanticHashCode()
    {
        return Identifier.GetSemanticHashCode();
    }

    #endregion

    #region IEquatable<Annotation> Implementation

    /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
    public bool Equals(Annotation? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Identifier.Equals(other.Identifier) && Arguments.Equals(other.Arguments) && Start == other.Start &&
               End == other.End && Source == other.Source;
    }

    #endregion
}
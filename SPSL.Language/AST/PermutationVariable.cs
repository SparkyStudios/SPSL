namespace SPSL.Language.AST;

/// <summary>
/// An SPSL shader permutation variable.
/// </summary>
public class PermutationVariable : INamespaceChild, IBlockChild
{
    public enum VariableType
    {
        Bool,
        Enum,
        Integer
    }

    #region Properties

    /// <summary>
    /// The type of the variable.
    /// </summary>
    public VariableType Type { get; set; }

    /// <summary>
    /// The initial value of the variable.
    /// </summary>
    public IExpression Initializer { get; set; }

    /// <summary>
    /// An array of enumeration values in case of an enumerated permutation value.
    /// </summary>
    public string[] EnumerationValues { get; set; } = Array.Empty<string>();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PermutationVariable"/> class.
    /// </summary>
    public PermutationVariable(VariableType type, IExpression initializer)
    {
        Type = type;
        Initializer = initializer;
    }

    #endregion

    #region INamespaceChild Implementation

    /// <summary>
    /// The parent <see cref="Language.AST.Namespace"/> of this one.
    /// Defaults to <c>null</c> for root namespaces.
    /// </summary>
    public Namespace? Parent { get; set; }

    /// <summary>
    /// The type name.
    /// </summary>
    public string Name { get; set; } = null!;

    #endregion
}
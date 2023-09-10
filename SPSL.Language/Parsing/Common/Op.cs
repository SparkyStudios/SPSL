namespace SPSL.Language.Parsing.Common;

/// <summary>
/// Represents an operator in the SPSL language.
/// </summary>
public enum Op
{
    /// <summary>
    /// An unknown operator. This value usually means that the operator was not recognized
    /// during parsing the source code.
    /// </summary>
    Unknown,
    
    /// <summary>
    /// The `pipe` operator (`|`).
    /// </summary>
    Pipe,

    /// <summary>
    /// The `ampersand` operator (`&`).
    /// </summary>
    Ampersand,

    /// <summary>
    /// The `plus` operator (`+`).
    /// </summary>
    Plus,

    /// <summary>
    /// The `minus` operator (`-`).
    /// </summary>
    Minus,

    /// <summary>
    /// The `star` operator (`*`).
    /// </summary>
    Star,
    Asterisk = Star,

    /// <summary>
    /// The `exponent` operator (`^`).
    /// </summary>
    Exponent,

    /// <summary>
    /// The `modulo` operator (`%`).
    /// </summary>
    Modulo,

    /// <summary>
    /// The `division` operator (`/`).
    /// </summary>
    Div,

    /// <summary>
    /// The `assignment` operator (`=`).
    /// </summary>
    Assignment,

    /// <summary>
    /// The `equals` operator (`==`).
    /// </summary>
    Equals,

    /// <summary>
    /// The `not equals` operator (`!=`).
    /// </summary>
    NotEquals,
    Different = NotEquals,

    /// <summary>
    /// The `less than` operator (`<`).
    /// </summary>
    LessThan,

    /// <summary>
    /// The `greater than` operator (`>`).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// The `less than or equal` operator (`<=`).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// The `greater than or equal` operator (`>=`).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// The `increment` operator (`++`).
    /// </summary>
    Increment,

    /// <summary>
    /// The `decrement` operator (`--`).
    /// </summary>
    Decrement,

    /// <summary>
    /// The `plus assignment` operator (`+=`).
    /// </summary>
    PlusAssignment,

    /// <summary>
    /// The `minus assignment` operator (`-=`).
    /// </summary>
    MinusAssignment,

    /// <summary>
    /// The `multiply assignment` operator (`*=`).
    /// </summary>
    AsteriskAssignment,

    /// <summary>
    /// The `divide assignment` operator (`/=`).
    /// </summary>
    DivideAssignment,

    /// <summary>
    /// The `modulo assignment` operator (`%=`).
    /// </summary>
    ModuloAssignment,

    /// <summary>
    /// The `bitwise or assignment` operator (`|=`).
    /// </summary>
    BitwiseOrAssignment,

    /// <summary>
    /// The `bitwise and assignment` operator (`&=`).
    /// </summary>
    BitwiseAndAssignment,

    /// <summary>
    /// The `exponent assignment` operator (`^=`).
    /// </summary>
    ExponentAssignment,

    /// <summary>
    /// The `left shift assignment` operator (`<<=`).
    /// </summary>
    LeftShiftAssignment,

    /// <summary>
    /// The `right shift assignment` operator (`>>=`).
    /// </summary>
    RightShiftAssignment,

    /// <summary>
    /// The `or` operator (`||`).
    /// </summary>
    Or,

    /// <summary>
    /// The `and` operator (`&&`).
    /// </summary>
    And,

    /// <summary>
    /// The `not` operator (`!`).
    /// </summary>
    Not,

    /// <summary>
    /// The `xor` operator (`^^`).
    /// </summary>
    Xor,

    /// <summary>
    /// The `left shift` operator (`<<`).
    /// </summary>
    LeftShift,

    /// <summary>
    /// The `right shift` operator (`>>`).
    /// </summary>
    RightShift,
}
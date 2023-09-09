using System.Text.RegularExpressions;
using Antlr4.Runtime;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Visitors;
using static SPSL.Language.SPSLParser;

namespace SPSL.Language.Utils;

public static partial class ParserExtensions
{
    internal static string ToDocumentation(this IToken? node)
    {
        Regex regex = StripDocumentationStarRegex();

        return regex.Replace(node?.Text
            .ReplaceLineEndings("\n")
            .Replace("/**", "")
            .Replace("*/", "")
            .Trim() ?? string.Empty, "\n");
    }

    internal static Identifier ToIdentifier(this IToken token, string fileSource)
    {
        return new()
        {
            Start = token.StartIndex,
            End = token.StopIndex,
            Value = token.Text,
            Source = fileSource
        };
    }

    internal static NamespacedReference ToNamespaceReference(this NamespacedTypeNameContext? context, string fileSource)
    {
        return context is not null
            ? new(context.IDENTIFIER().Select(id => id.Symbol.ToIdentifier(fileSource)).ToArray())
            {
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Source = fileSource
            }
            : NamespacedReference.Null;
    }

    internal static Annotation ToAnnotation(this AnnotationContext context, string fileSource)
    {
        return new
        (
            context.Name.ToIdentifier(fileSource),
            context.constantExpression()
                .Select(e => e.Accept(new ExpressionVisitor(fileSource))!)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static Function ToFunction(this FunctionContext context, string fileSource)
    {
        return new
        (
            context.Head.ToFunctionHead(fileSource),
            context.Body.ToStatementBlock(fileSource)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static FunctionHead ToFunctionHead(this FunctionHeadContext context, string fileSource)
    {
        return new
        (
            context.Type.Accept(new DataTypeVisitor(fileSource)),
            context.Name.ToIdentifier(fileSource),
            context.Signature.ToFunctionSignature(fileSource)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    internal static FunctionSignature ToFunctionSignature(this FunctionSignatureContext context, string fileSource)
    {
        FunctionSignature signature = new()
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };

        if (context.Arguments == null) return signature;

        foreach (ArgDefContext arg in context.Arguments.argDef())
        {
            signature.AddParameter
            (
                new
                (
                    arg.Flow?.Text switch
                    {
                        "in" => DataFlow.In,
                        "out" => DataFlow.Out,
                        "inout" => DataFlow.InOut,
                        "const" => DataFlow.Const,
                        _ => DataFlow.Unspecified
                    },
                    arg.Type.Accept(new DataTypeVisitor(fileSource)),
                    arg.Name.ToIdentifier(fileSource)
                )
                {
                    Start = arg.Start.StartIndex,
                    End = arg.Stop.StopIndex,
                    Source = fileSource,
                    Documentation = arg.Documentation.ToDocumentation()
                }
            );
        }

        return signature;
    }

    internal static StatementBlock ToStatementBlock(this FunctionBodyContext context, string fileSource)
    {
        OrderedSet<IStatement> statements = new();
        StatementVisitor statementVisitor = new(fileSource);

        foreach (StayControlFlowStatementContext statement in context.stayControlFlowStatement())
            statements.Add(statement.Accept(statementVisitor)!);

        if (context.ReturnStatement != null)
            statements.Add(context.ReturnStatement.Accept(statementVisitor)!);

        return new(statements)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this PermutationVariableContext context,
        string fileSource
    )
    {
        if (context.permutationVariableBool() != null)
            return context.permutationVariableBool().ToPermutationVariable(fileSource);

        if (context.permutationVariableEnum() != null)
            return context.permutationVariableEnum().ToPermutationVariable(fileSource);

        if (context.permutationVariableInteger() != null)
            return context.permutationVariableInteger().ToPermutationVariable(fileSource);

        throw new ArgumentException("The provided context is not valid.");
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this PermutationVariableBoolContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariableType.Bool,
            context.Identifier.Identifier.ToIdentifier(fileSource),
            new BoolLiteral(bool.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex,
                Source = fileSource
            }
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this PermutationVariableEnumContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariableType.Enum,
            context.Identifier.Identifier.ToIdentifier(fileSource),
            context.Value.Accept(new ExpressionVisitor(fileSource))!
        )
        {
            EnumerationValues = context.IDENTIFIER().Select(id => id.Symbol.ToIdentifier(fileSource)).ToArray(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this PermutationVariableIntegerContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariableType.Integer,
            context.Identifier.Identifier.ToIdentifier(fileSource),
            new IntegerLiteral(int.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex,
                Source = fileSource
            }
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    internal static TypeProperty ToTypeProperty(this BufferComponentContext context, string fileSource)
    {
        return new
        (
            context.Type.Accept(new DataTypeVisitor(fileSource)),
            context.Name.ToIdentifier(fileSource)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    public static BufferStorage ToBufferStorage(this IToken? storage)
    {
        return storage?.Type switch
        {
            KEYWORD_COHERENT => BufferStorage.Coherent,
            _ => BufferStorage.Undefined
        };
    }

    public static BufferAccess ToBufferAccess(this IToken? access)
    {
        return access?.Type switch
        {
            KEYWORD_READONLY => BufferAccess.ReadOnly,
            KEYWORD_WRITEONLY => BufferAccess.WriteOnly,
            KEYWORD_READWRITE => BufferAccess.ReadWrite,
            SPSLLexer.KEYWORD_CONST => BufferAccess.Constant,
            _ => BufferAccess.ReadOnly
        };
    }

    public static ShaderStage ToShaderStage(this IToken? stage)
    {
        return stage?.Type switch
        {
            KEYWORD_VERTEX => ShaderStage.Vertex,
            KEYWORD_PIXEL => ShaderStage.Pixel,
            KEYWORD_GEOMETRY => ShaderStage.Geometry,
            KEYWORD_HULL => ShaderStage.Hull,
            KEYWORD_DOMAIN => ShaderStage.Domain,
            KEYWORD_COMPUTE => ShaderStage.Compute,
            _ => ShaderStage.Unspecified,
        };
    }

    public static DataFlow ToDataFlow(this IToken? flow)
    {
        return flow?.Type switch
        {
            KEYWORD_IN => DataFlow.In,
            KEYWORD_OUT => DataFlow.Out,
            KEYWORD_INOUT => DataFlow.InOut,
            KEYWORD_CONST => DataFlow.Const,
            _ => DataFlow.Unspecified
        };
    }

    public static Op ToOp(this IToken? op)
    {
        return op?.Type switch
        {
            OP_PIPE => Op.Pipe,
            OP_AMPERSAND => Op.Ampersand,
            OP_PLUS => Op.Plus,
            OP_MINUS => Op.Minus,
            OP_ASTERISK => Op.Asterisk,
            OP_EXPONENT => Op.Exponent,
            OP_MODULUS => Op.Modulo,
            OP_DIV => Op.Div,
            OP_ASSIGN => Op.Assignment,
            OP_EQUAL => Op.Equals,
            OP_DIFFERENT => Op.NotEquals,
            OP_GREATER_THAN => Op.GreaterThan,
            OP_LESSER_THAN => Op.LessThan,
            OP_GEQ_THAN => Op.GreaterThanOrEqual,
            OP_LEQ_THAN => Op.LessThanOrEqual,
            OP_INCREMENT => Op.Increment,
            OP_DECREMENT => Op.Decrement,
            OP_PLUS_ASSIGN => Op.PlusAssignment,
            OP_MINUS_ASSIGN => Op.MinusAssignment,
            OP_MUL_ASSIGN => Op.AsteriskAssignment,
            OP_DIV_ASSIGN => Op.DivideAssignment,
            OP_MODULUS_ASSIGN => Op.ModuloAssignment,
            OP_BITWISE_OR_ASSIGN => Op.BitwiseOrAssignment,
            OP_BITWISE_AND_ASSIGN => Op.BitwiseAndAssignment,
            OP_EXPONENT_ASSIGN => Op.ExponentAssignment,
            OP_LSHIFT_ASSIGN => Op.LeftShiftAssignment,
            OP_RSHIFT_ASSIGN => Op.RightShiftAssignment,
            OP_OR => Op.Or,
            OP_AND => Op.And,
            OP_NOT => Op.Not,
            OP_XOR => Op.Xor,
            OP_LSHIFT => Op.LeftShift,
            OP_RSHIFT => Op.RightShift,
            _ => Op.Unknown
        };
    }

    [GeneratedRegex(@"(\s*\*)(?:[\s\n\r])?")]
    private static partial Regex StripDocumentationStarRegex();
}
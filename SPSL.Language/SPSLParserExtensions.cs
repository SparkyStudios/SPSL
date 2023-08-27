using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Utils;
using SPSL.Language.Visitors;
using static SPSL.Language.SPSLParser;

namespace SPSL.Language;

internal static class SPSLParserExtensions
{
    internal static string ToDocumentation(this IToken? node)
    {
        return node?.Text
            .Replace("\r\n", "\n")
            .Replace("/**", "")
            .Replace("*/", "")
            .Replace("* ", "")
            .Trim() ?? string.Empty;
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

    internal static NamespacedReference ToNamespaceReference
    (
        this NamespacedTypeNameContext? context,
        string fileSource
    )
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
        Annotation annotation = new()
        {
            Identifier = context.Name.ToIdentifier(fileSource),
            Arguments = new(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };

        annotation.Identifier.Parent = annotation;

        annotation.Arguments.AddRange
        (
            context.constantExpression()
                .Select(e =>
                {
                    IExpression expression = e.Accept(new ExpressionVisitor(fileSource))!;
                    expression.Parent = annotation;
                    return expression;
                })
        );

        return annotation;
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

    internal static FunctionSignature ToFunctionSignature
    (
        this FunctionSignatureContext context,
        string fileSource
    )
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
}
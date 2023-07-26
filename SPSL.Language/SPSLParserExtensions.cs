using SPSL.Language.AST;
using SPSL.Language.Utils;
using SPSL.Language.Visitors;

namespace SPSL.Language;

internal static class SPSLParserExtensions
{
    internal static Annotation ToAnnotation(this SPSLParser.AnnotationContext context, string fileSource)
    {
        return new()
        {
            Name = context.Name.Text,
            Arguments = new(context.constantExpression().Select(e => e.Accept(new ExpressionVisitor(fileSource))!)),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static Function ToFunction(this SPSLParser.FunctionContext context, string fileSource)
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

    internal static FunctionHead ToFunctionHead(this SPSLParser.FunctionHeadContext context, string fileSource)
    {
        return new
        (
            context.Type.Accept(new DataTypeVisitor(fileSource)),
            context.Name.Text,
            context.Signature.ToFunctionSignature(fileSource)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static FunctionSignature ToFunctionSignature
    (
        this SPSLParser.FunctionSignatureContext context,
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

        foreach (SPSLParser.ArgDefContext arg in context.Arguments.argDef())
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
                    arg.Name.Text
                )
                {
                    Start = arg.Start.StartIndex,
                    End = arg.Stop.StopIndex,
                    Source = fileSource
                }
            );
        }

        return signature;
    }

    internal static StatementBlock ToStatementBlock(this SPSLParser.FunctionBodyContext context, string fileSource)
    {
        OrderedSet<IStatement> statements = new();
        StatementVisitor statementVisitor = new(fileSource);

        foreach (SPSLParser.StayControlFlowStatementContext statement in context.stayControlFlowStatement())
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
        this SPSLParser.PermutationVariableContext context,
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
        this SPSLParser.PermutationVariableBoolContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariable.VariableType.Bool,
            new BoolLiteral(bool.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex,
                Source = fileSource
            }
        )
        {
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this SPSLParser.PermutationVariableEnumContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariable.VariableType.Enum,
            context.Value.Accept(new ExpressionVisitor(fileSource))!
        )
        {
            EnumerationValues = context.IDENTIFIER().Select(id => id.GetText()).ToArray(),
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static PermutationVariable ToPermutationVariable
    (
        this SPSLParser.PermutationVariableIntegerContext context,
        string fileSource
    )
    {
        return new
        (
            PermutationVariable.VariableType.Integer,
            new IntegerLiteral(int.Parse(context.Value.Text))
            {
                Start = context.Value.StartIndex,
                End = context.Value.StopIndex,
                Source = fileSource
            }
        )
        {
            Name = context.Identifier.GetText(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }

    internal static TypeProperty ToTypeProperty(this SPSLParser.BufferComponentContext context, string fileSource)
    {
        return new
        (
            context.Type.Accept(new DataTypeVisitor(fileSource)),
            context.Name.Text
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = fileSource
        };
    }
}
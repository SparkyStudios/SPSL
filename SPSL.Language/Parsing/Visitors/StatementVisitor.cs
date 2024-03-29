using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Utils;
using SPSL.Language.Utils;
using static SPSL.Language.Core.SPSLParser;

namespace SPSL.Language.Parsing.Visitors;

public class StatementVisitor : SPSLBaseVisitor<IStatement?>
{
    private readonly string _fileSource;

    public StatementVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override IStatement? DefaultResult => null;

    protected override IStatement? AggregateResult(IStatement? aggregate, IStatement? nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IStatement? currentResult)
    {
        switch (node)
        {
            case StatementContext:
            case StayControlFlowStatementContext:
            case LeaveControlFlowStatementContext:

            case VariableDeclarationContext:
            case ExpressionStatementContext:
            case StatementBlockContext:
            case IfStatementContext:
            case SwitchStatementContext:
            case WhileStatementContext:
            case DoWhileStatementContext:
            case PermuteStatementContext:

            case BreakStatementContext:
            case ReturnStatementContext:
            case ContinueStatementContext:
            case DiscardStatementContext:
                return true;

            // Not parsed
            default:
                return false;
        }
    }

    public override IStatement VisitStatement([NotNull] StatementContext context)
    {
        if (context.StayControlFlowStatement != null)
            return context.StayControlFlowStatement.Accept(this)!;

        if (context.LeaveControlFlowStatement != null)
            return context.LeaveControlFlowStatement.Accept(this)!;

        throw new NotSupportedException();
    }

    public override IStatement VisitStayControlFlowStatement([NotNull] StayControlFlowStatementContext context)
    {
        if (context.VariableDeclaration != null)
            return context.VariableDeclaration.Accept(this)!;

        if (context.ExpressionStatement != null)
            return new ExpressionStatement(context.ExpressionStatement.Accept(new ExpressionVisitor(_fileSource))!)
            {
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Source = _fileSource
            };

        if (context.StatementBlock != null)
            return context.StatementBlock.Accept(this)!;

        if (context.IfStatement != null)
            return context.IfStatement.Accept(this)!;

        if (context.SwitchStatement != null)
            return context.SwitchStatement.Accept(this)!;

        if (context.WhileStatement != null)
            return context.WhileStatement.Accept(this)!;

        if (context.DoWhileStatement != null)
            return context.DoWhileStatement.Accept(this)!;

        if (context.PermuteStatement != null)
            return context.PermuteStatement.Accept(this)!;

        return new InvalidStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitLeaveControlFlowStatement([NotNull] LeaveControlFlowStatementContext context)
    {
        if (context.BreakStatement != null)
            return context.BreakStatement.Accept(this)!;

        if (context.ReturnStatement != null)
            return context.ReturnStatement.Accept(this)!;

        if (context.ContinueStatement != null)
            return context.ContinueStatement.Accept(this)!;

        if (context.DiscardStatement != null)
            return context.DiscardStatement.Accept(this)!;

        throw new NotSupportedException();
    }

    public override IStatement VisitTypedVariableDeclaration([NotNull] TypedVariableDeclarationContext context)
    {
        OrderedSet<VariableDeclarationStatement> declarations = new();
        IDataType type = context.Type.Accept(new DataTypeVisitor(_fileSource))!;
        ExpressionVisitor expressionVisitor = new(_fileSource);

        foreach (VariableIdentityContext variableIdentity in context.variableIdentity())
        {
            IExpression? value = null;
            BasicExpressionContext identifier = variableIdentity.Identifier;
            if (variableIdentity.IsAssignment)
            {
                identifier = variableIdentity.Declaration.Identifier;
                value = variableIdentity.Declaration.Expression.Accept(expressionVisitor)!;
            }

            declarations.Add
            (
                new(type, identifier.Identifier.ToIdentifier(_fileSource), value)
                {
                    IsConst = context.IsConst,
                    Start = variableIdentity.Start.StartIndex,
                    End = variableIdentity.Stop.StopIndex,
                    Source = _fileSource
                }
            );
        }

        return new StatementCollection(declarations)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitUntypedVariableDeclaration([NotNull] UntypedVariableDeclarationContext context)
    {
        ExpressionVisitor visitor = new(_fileSource);

        return new VariableDeclarationStatement
        (
            type: new UnknownDataType
            {
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Source = _fileSource
            },
            name: context.Identifier.Identifier.ToIdentifier(_fileSource),
            initializer: context.Expression.Accept(visitor)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitStatementBlock([NotNull] StatementBlockContext context)
    {
        StatementBlock block = new()
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };

        foreach (StatementContext statement in context.statement())
            block.Children.Add(statement.Accept(this)!);

        return block;
    }

    public override IStatement VisitIfStatement([NotNull] IfStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        IfStatement.IfStatementConditionBlock @if = new()
        {
            Condition = context.Expression.Accept(expressionVisitor),
            Block = (StatementBlock)context.Block.Accept(this)!,
            Start = context.Block.Start.StartIndex,
            End = context.Block.Stop.StartIndex,
            Source = _fileSource
        };

        OrderedSet<IfStatement.IfStatementConditionBlock> elif = new();
        foreach (ElifStatementContext ctx in context.elifStatement())
        {
            elif.Add
            (
                new()
                {
                    Condition = ctx.Expression.Accept(expressionVisitor)!,
                    Block = (StatementBlock)ctx.Block.Accept(this)!,
                    Start = ctx.Block.Start.StartIndex,
                    End = ctx.Block.Stop.StartIndex,
                    Source = _fileSource
                }
            );
        }

        StatementBlock? @else = null;
        if (context.Else != null)
            @else = (StatementBlock?)context.Else.Block.Accept(this);

        return new IfStatement(@if, elif, @else)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitWhileStatement([NotNull] WhileStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        return new WhileStatement
        (
            condition: context.Expression.Accept(expressionVisitor),
            block: (StatementBlock)context.Block.Accept(this)!
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitDoWhileStatement(DoWhileStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        return new WhileStatement
        (
            condition: context.Expression.Accept(expressionVisitor),
            block: (StatementBlock)context.Block.Accept(this)!,
            isDoWhile: true
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitPermuteStatement([NotNull] PermuteStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        BinaryOperationExpression condition = new
        (
            context.Identifier.Accept(expressionVisitor),
            context.Operator.ToOp(),
            context.Value.Accept(expressionVisitor)
        )
        {
            Start = context.Identifier.Start.StartIndex,
            End = context.Value.Stop.StopIndex,
            Source = _fileSource
        };

        var block = (StatementBlock)context.Block.Accept(this)!;

        StatementBlock? @else = null;
        if (context.Else != null)
            @else = (StatementBlock?)context.Else.Block.Accept(this);

        return new PermuteStatement(condition, block, @else)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitBreakStatement([NotNull] BreakStatementContext context)
    {
        return new BreakStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitReturnStatement([NotNull] ReturnStatementContext context)
    {
        return new ReturnStatement(context.Expression?.Accept(new ExpressionVisitor(_fileSource)))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitContinueStatement([NotNull] ContinueStatementContext context)
    {
        return new ContinueStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitDiscardStatement([NotNull] DiscardStatementContext context)
    {
        return new DiscardStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }
}
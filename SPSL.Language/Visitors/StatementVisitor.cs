using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;

namespace SPSL.Language.Visitors;

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
            case SPSLParser.StatementContext _:
            case SPSLParser.StayControlFlowStatementContext _:
            case SPSLParser.LeaveControlFlowStatementContext _:

            case SPSLParser.VariableDeclarationContext _:
            case SPSLParser.ExpressionStatementContext _:
            case SPSLParser.StatementBlockContext _:
            case SPSLParser.IfStatementContext _:
            case SPSLParser.SwitchStatementContext _:
            case SPSLParser.WhileStatementContext _:

            case SPSLParser.BreakStatementContext _:
            case SPSLParser.ReturnStatementContext _:
            case SPSLParser.ContinueStatementContext _:
            case SPSLParser.DiscardStatementContext _:
                return true;

            // Not parsed
            default:
                return false;
        }
    }

    public override IStatement VisitStatement(SPSLParser.StatementContext context)
    {
        if (context.StayControlFlowStatement != null)
            return context.StayControlFlowStatement.Accept(this)!;

        if (context.LeaveControlFlowStatement != null)
            return context.LeaveControlFlowStatement.Accept(this)!;

        throw new NotSupportedException();
    }

    public override IStatement VisitStayControlFlowStatement(SPSLParser.StayControlFlowStatementContext context)
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

        if (context.PermuteStatement != null)
            return context.PermuteStatement.Accept(this)!;

        return new InvalidStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitLeaveControlFlowStatement(SPSLParser.LeaveControlFlowStatementContext context)
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

    public override IStatement VisitTypedVariableDeclaration(SPSLParser.TypedVariableDeclarationContext context)
    {
        OrderedSet<VariableDeclarationStatement> declarations = new();
        IDataType type = context.Type.Accept(new DataTypeVisitor(_fileSource))!;
        ExpressionVisitor expressionVisitor = new(_fileSource);

        foreach (SPSLParser.VariableIdentityContext variableIdentity in context.variableIdentity())
        {
            IExpression? value = null;
            SPSLParser.BasicExpressionContext identifier = variableIdentity.Identifier;
            if (variableIdentity.IsAssignment)
            {
                identifier = variableIdentity.Declaration.Identifier;
                value = variableIdentity.Declaration.Expression.Accept(expressionVisitor)!;
            }

            declarations.Add
            (
                new()
                {
                    IsConst = context.IsConst,
                    Type = type,
                    Name = (BasicExpression)identifier.Accept(expressionVisitor)!,
                    Initializer = value,
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

    public override IStatement VisitUntypedVariableDeclaration(SPSLParser.UntypedVariableDeclarationContext context)
    {
        ExpressionVisitor visitor = new(_fileSource);

        return new VariableDeclarationStatement
        {
            Type = new UnknownDataType
            {
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Source = _fileSource
            },
            Name = (BasicExpression)context.Declaration.Identifier.Accept(visitor)!,
            Initializer = context.Declaration.Expression.Accept(visitor),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitStatementBlock(SPSLParser.StatementBlockContext context)
    {
        StatementBlock block = new()
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };

        foreach (SPSLParser.StatementContext statement in context.statement())
            block.Children.Add(statement.Accept(this)!);

        return block;
    }

    public override IStatement VisitIfStatement(SPSLParser.IfStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        IfStatement.IfStatementConditionBlock @if = new()
        {
            Condition = context.Expression.Accept(expressionVisitor)!,
            Block = (StatementBlock)context.Block.Accept(this)!
        };

        OrderedSet<IfStatement.IfStatementConditionBlock> elif = new();
        foreach (SPSLParser.ElifStatementContext ctx in context.elifStatement())
        {
            elif.Add
            (
                new IfStatement.IfStatementConditionBlock
                {
                    Condition = ctx.Expression.Accept(expressionVisitor)!,
                    Block = (StatementBlock)ctx.Block.Accept(this)!,
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

    public override IStatement VisitWhileStatement(SPSLParser.WhileStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        return new WhileStatement
        {
            Condition = context.Expression.Accept(expressionVisitor)!,
            Block = (StatementBlock)context.Block.Accept(this)!,
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitPermuteStatement(SPSLParser.PermuteStatementContext context)
    {
        ExpressionVisitor expressionVisitor = new(_fileSource);

        BinaryOperationExpression condition = new
        (
            context.Identifier.Accept(expressionVisitor)!,
            context.Operator.Text,
            context.Value.Accept(expressionVisitor)!
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

    public override IStatement VisitBreakStatement(SPSLParser.BreakStatementContext context)
    {
        return new BreakStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitReturnStatement(SPSLParser.ReturnStatementContext context)
    {
        return new ReturnStatement(context.Expression.Accept(new ExpressionVisitor(_fileSource)))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitContinueStatement(SPSLParser.ContinueStatementContext context)
    {
        return new ContinueStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }

    public override IStatement VisitDiscardStatement(SPSLParser.DiscardStatementContext context)
    {
        return new DiscardStatement
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StartIndex,
            Source = _fileSource
        };
    }
}
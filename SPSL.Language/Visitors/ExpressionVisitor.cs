using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;
using static SPSL.Language.SPSLParser;

namespace SPSL.Language.Visitors;

public class ExpressionVisitor : SPSLBaseVisitor<IExpression>
{
    private readonly string _fileSource;

    public ExpressionVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override IExpression DefaultResult => new InvalidExpression
    {
        Source = _fileSource
    };

    protected override IExpression AggregateResult(IExpression aggregate, IExpression nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IExpression currentResult)
    {
        return node is ExpressionStatementContext or BasicExpressionContext
            or ParenthesizedExpressionContext or PrimitiveExpressionContext
            or ConstantExpressionContext or MethodMemberReferenceExpressionContext
            or InvocationExpressionContext or ArrayAccessExpressionContext
            or NewInstanceExpressionContext or AssignableExpressionContext
            or ChainedExpressionContext or AssignableChainedExpressionContext
            or ChainableExpressionContext or PropertyMemberReferenceExpressionContext
            or ContextAccessExpressionContext or MemberReferenceExpressionContext
            or ReferencableExpressionContext or AssignableChainableExpressionContext;
    }

    public override IExpression VisitBasicExpression(BasicExpressionContext context)
    {
        return new BasicExpression(context.Identifier.ToIdentifier(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitParenthesizedExpression(ParenthesizedExpressionContext context)
    {
        return new ParenthesizedExpression(context.Expression.Accept(this))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitPrimitiveExpression(PrimitiveExpressionContext context)
    {
        return context.Accept(new LiteralVisitor(_fileSource))!;
    }

    public override IExpression VisitPrimitiveConstantExpression(PrimitiveConstantExpressionContext context)
    {
        return context.Accept(new LiteralVisitor(_fileSource))!;
    }

    public override IExpression VisitUserDefinedConstantExpression
    (
        UserDefinedConstantExpressionContext context
    )
    {
        return new UserDefinedConstantExpression(context.namespacedTypeName().ToNamespaceReference(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitPropertyMemberReferenceExpression
    (
        PropertyMemberReferenceExpressionContext context
    )
    {
        return new PropertyMemberReferenceExpression
        (
            (ContextAccessExpression)context.Target.Accept(this),
            context.Member.Identifier.ToIdentifier(_fileSource)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitMethodMemberReferenceExpression
    (
        MethodMemberReferenceExpressionContext context
    )
    {
        return new MethodMemberReferenceExpression
        (
            (ContextAccessExpression)context.Target.Accept(this),
            (InvocationExpression)context.Member.Accept(this)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitChainedExpression
    (
        ChainedExpressionContext context
    )
    {
        return new ChainedExpression
        (
            context.Target.Accept(this),
            context.chainableExpression().Select(member => member.Accept(this))
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitAssignableChainedExpression
    (
        AssignableChainedExpressionContext context
    )
    {
        return new ChainedExpression
        (
            context.Target.Accept(this),
            context.assignableChainableExpression().Select(member => member.Accept(this))
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitContextAccessExpression(ContextAccessExpressionContext context)
    {
        return new ContextAccessExpression(context.Identifier.ToIdentifier(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitInvocationExpression(InvocationExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            foreach (ExpressionStatementContext expression in context.Parameters.expressionStatement())
            {
                parameters.Add
                (
                    new(expression.Accept(this))
                    {
                        Start = expression.Start.StartIndex,
                        End = expression.Stop.StopIndex,
                        Source = _fileSource
                    }
                );
            }
        }

        return new InvocationExpression(context.Name.ToNamespaceReference(_fileSource), parameters)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitArrayAccessExpression(ArrayAccessExpressionContext context)
    {
        ParserRuleContext access;

        if ((access = context.basicExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this), context.Index.Accept(this))
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex,
                Source = _fileSource
            };

        if ((access = context.memberReferenceExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this), context.Index.Accept(this))
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex,
                Source = _fileSource
            };

        if ((access = context.invocationExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this), context.Index.Accept(this))
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex,
                Source = _fileSource
            };

        throw new NotSupportedException();
    }

    public override IExpression VisitNegateOperationExpression(NegateOperationExpressionContext context)
    {
        return new NegateOperationExpression(context.Expression.Accept(this))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitPostfixUnaryOperationExpression
    (
        PostfixUnaryOperationExpressionContext context
    )
    {
        return new UnaryOperationExpression
        (
            (IAssignableExpression)context.Expression.Accept(this),
            context.Operator.ToOp(),
            true
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitPrefixUnaryOperationExpression
    (
        PrefixUnaryOperationExpressionContext context
    )
    {
        return new UnaryOperationExpression
        (
            (IAssignableExpression)context.Expression.Accept(this),
            context.Operator.ToOp()
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitSignedExpression(SignedExpressionContext context)
    {
        return new SignedExpression(context.Operator.ToOp(), context.Expression.Accept(this))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitBinaryOperationExpression(BinaryOperationExpressionContext context)
    {
        return new BinaryOperationExpression
        (
            context.Left.Accept(this),
            context.Operator.ToOp(),
            context.Right.Accept(this)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitTernaryOperationExpression(TernaryOperationExpressionContext context)
    {
        return new TernaryOperationExpression
        (
            context.Condition.Accept(this),
            context.WhenTrue.Accept(this),
            context.WhenFalse.Accept(this)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitAssignmentExpression(AssignmentExpressionContext context)
    {
        return new AssignmentExpression
        (
            (IAssignableExpression)context.Left.Accept(this),
            context.Operator.ToOp(),
            context.Right.Accept(this)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitCastExpression(CastExpressionContext context)
    {
        return new CastExpression
        (
            context.Type.Accept(new DataTypeVisitor(_fileSource)),
            context.Expression.Accept(this)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IExpression VisitNewInstanceExpression(NewInstanceExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            foreach (ExpressionStatementContext expression in context.Parameters.expressionStatement())
            {
                parameters.Add
                (
                    new(expression.Accept(this))
                    {
                        Start = expression.Start.StartIndex,
                        End = expression.Stop.StopIndex,
                        Source = _fileSource
                    }
                );
            }
        }

        return new NewInstanceExpression(context.Type.Accept(new DataTypeVisitor(_fileSource))!, parameters)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }
}
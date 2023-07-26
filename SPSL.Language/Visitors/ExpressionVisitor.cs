using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Utils;

namespace SPSL.Language.Visitors;

public class ExpressionVisitor : SPSLBaseVisitor<IExpression?>
{
    protected override IExpression? DefaultResult => null;

    protected override IExpression? AggregateResult(IExpression? aggregate, IExpression? nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IExpression? currentResult)
    {
        return node is SPSLParser.ExpressionStatementContext or SPSLParser.BasicExpressionContext
            or SPSLParser.ParenthesizedExpressionContext or SPSLParser.PrimitiveExpressionContext
            or SPSLParser.ConstantExpressionContext or SPSLParser.MethodMemberReferenceExpressionContext
            or SPSLParser.InvocationExpressionContext or SPSLParser.ArrayAccessExpressionContext
            or SPSLParser.NewInstanceExpressionContext or SPSLParser.AssignableExpressionContext
            or SPSLParser.ChainedExpressionContext or SPSLParser.AssignableChainedExpressionContext
            or SPSLParser.ChainableExpressionContext or SPSLParser.PropertyMemberReferenceExpressionContext
            or SPSLParser.ContextAccessExpressionContext or SPSLParser.MemberReferenceExpressionContext
            or SPSLParser.ReferencableExpressionContext or SPSLParser.AssignableChainableExpressionContext;
    }

    public override IExpression VisitBasicExpression(SPSLParser.BasicExpressionContext context)
    {
        return new BasicExpression(context.Identifier.Text)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitParenthesizedExpression(SPSLParser.ParenthesizedExpressionContext context)
    {
        return new ParenthesizedExpression(context.Expression.Accept(this)!)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitPrimitiveExpression(SPSLParser.PrimitiveExpressionContext context)
    {
        return context.Accept(new LiteralVisitor())!;
    }

    public override IExpression VisitPrimitiveConstantExpression(
        SPSLParser.PrimitiveConstantExpressionContext context)
    {
        return context.Accept(new LiteralVisitor())!;
    }

    public override IExpression VisitUserDefinedConstantExpression(
        SPSLParser.UserDefinedConstantExpressionContext context)
    {
        return new UserDefinedConstantExpression(new(context.namespacedTypeName().GetText()))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitPropertyMemberReferenceExpression(
        SPSLParser.PropertyMemberReferenceExpressionContext context)
    {
        return new PropertyMemberReferenceExpression
        (
            context.Target.Text,
            context.Member.Identifier.Text
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitMethodMemberReferenceExpression
    (
        SPSLParser.MethodMemberReferenceExpressionContext context
    )
    {
        return new MethodMemberReferenceExpression
        (
            context.Target.Text,
            (InvocationExpression)context.Member.Accept(this)!
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitChainedExpression
    (
        SPSLParser.ChainedExpressionContext context
    )
    {
        return new ChainedExpression
        (
            context.Target.Accept(this)!,
            context.chainableExpression().Select(member => member.Accept(this)!)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitAssignableChainedExpression(
        SPSLParser.AssignableChainedExpressionContext context)
    {
        return new ChainedExpression
        (
            context.Target.Accept(this)!,
            context.assignableChainableExpression().Select(member => member.Accept(this)!)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitContextAccessExpression(SPSLParser.ContextAccessExpressionContext context)
    {
        return new ContextAccessExpression(context.Identifier.Text)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitInvocationExpression(SPSLParser.InvocationExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            foreach (SPSLParser.ExpressionStatementContext expression in context.Parameters.expressionStatement())
            {
                parameters.Add
                (
                    new(expression.Accept(this)!)
                    {
                        Start = expression.Start.StartIndex,
                        End = expression.Stop.StopIndex
                    }
                );
            }
        }

        return new InvocationExpression(new(context.Name.GetText()), parameters)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitArrayAccessExpression(SPSLParser.ArrayAccessExpressionContext context)
    {
        ParserRuleContext access;

        if ((access = context.basicExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!)
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex
            };

        if ((access = context.memberReferenceExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!)
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex
            };

        if ((access = context.invocationExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!)
            {
                Start = access.Start.StartIndex,
                End = access.Stop.StopIndex
            };

        throw new NotSupportedException();
    }

    public override IExpression VisitNegateOperationExpression(SPSLParser.NegateOperationExpressionContext context)
    {
        return new NegateOperationExpression(context.Expression.Accept(this)!)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitPostfixUnaryOperationExpression(
        SPSLParser.PostfixUnaryOperationExpressionContext context)
    {
        return new UnaryOperationExpression
        (
            (IAssignableExpression)context.Expression.Accept(this)!,
            context.Operator.Text,
            true
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitPrefixUnaryOperationExpression(
        SPSLParser.PrefixUnaryOperationExpressionContext context)
    {
        return new UnaryOperationExpression
        (
            (IAssignableExpression)context.Expression.Accept(this)!,
            context.Operator.Text,
            false
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitSignedExpression(SPSLParser.SignedExpressionContext context)
    {
        return new SignedExpression(context.Operator.Text, context.Expression.Accept(this)!)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitBinaryOperationExpression(SPSLParser.BinaryOperationExpressionContext context)
    {
        return new BinaryOperationExpression
        (
            context.Left.Accept(this)!,
            context.Operator.Text,
            context.Right.Accept(this)!
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitTernaryOperationExpression(SPSLParser.TernaryOperationExpressionContext context)
    {
        return new TernaryOperationExpression
        (
            context.Condition.Accept(this)!,
            context.WhenTrue.Accept(this)!,
            context.WhenFalse.Accept(this)!
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitAssignmentExpression(SPSLParser.AssignmentExpressionContext context)
    {
        return new AssignmentExpression
        (
            (IAssignableExpression)context.Left.Accept(this)!,
            context.Operator.Text,
            context.Right.Accept(this)!
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitCastExpression(SPSLParser.CastExpressionContext context)
    {
        return new CastExpression(context.Type.Accept(new DataTypeVisitor())!, context.Expression.Accept(this)!)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }

    public override IExpression VisitNewInstanceExpression(SPSLParser.NewInstanceExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            foreach (SPSLParser.ExpressionStatementContext expression in context.Parameters.expressionStatement())
            {
                parameters.Add
                (
                    new(expression.Accept(this)!)
                    {
                        Start = expression.Start.StartIndex,
                        End = expression.Stop.StopIndex
                    }
                );
            }
        }

        return new NewInstanceExpression(context.Type.Accept(new DataTypeVisitor())!, parameters)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }
}
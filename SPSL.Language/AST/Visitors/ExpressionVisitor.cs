using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SPSL.Language.Utils;

namespace SPSL.Language.AST.Visitors;

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
        return new BasicExpression(context.Identifier.Text);
    }

    public override IExpression VisitParenthesizedExpression(SPSLParser.ParenthesizedExpressionContext context)
    {
        return new ParenthesizedExpression(context.Expression.Accept(this)!);
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
        return new UserDefinedConstantExpression(new NamespacedReference(context.namespacedTypeName().GetText()));
    }

    public override IExpression VisitPropertyMemberReferenceExpression(
        SPSLParser.PropertyMemberReferenceExpressionContext context)
    {
        return new PropertyMemberReferenceExpression
        (
            context.Target.Text,
            context.Member.Identifier.Text
        );
    }

    public override IExpression VisitMethodMemberReferenceExpression(
        SPSLParser.MethodMemberReferenceExpressionContext context)
    {
        return new MethodMemberReferenceExpression
        (
            context.Target.Text,
            (InvocationExpression)context.Member.Accept(this)!
        );
    }

    public override IExpression VisitChainedExpression(
        SPSLParser.ChainedExpressionContext context)
    {
        return new ChainedExpression
        (
            context.Target.Accept(this)!,
            context.chainableExpression().Select(member => member.Accept(this)!)
        );
    }

    public override IExpression VisitAssignableChainedExpression(
        SPSLParser.AssignableChainedExpressionContext context)
    {
        return new ChainedExpression
        (
            context.Target.Accept(this)!,
            context.assignableChainableExpression().Select(member => member.Accept(this)!)
        );
    }

    public override IExpression VisitContextAccessExpression(SPSLParser.ContextAccessExpressionContext context)
    {
        return new ContextAccessExpression(context.Identifier.Text);
    }

    public override IExpression VisitInvocationExpression(SPSLParser.InvocationExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            parameters = new OrderedSet<InvocationParameter>();
            foreach (SPSLParser.ExpressionStatementContext expression in context.Parameters.expressionStatement())
                parameters.Add(new InvocationParameter(expression.Accept(this)!));
        }

        return new InvocationExpression(new NamespacedReference(context.Name.GetText()), parameters);
    }

    public override IExpression VisitArrayAccessExpression(SPSLParser.ArrayAccessExpressionContext context)
    {
        ParserRuleContext access;

        if ((access = context.basicExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!);

        if ((access = context.memberReferenceExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!);

        if ((access = context.invocationExpression()) != null)
            return new ArrayAccessExpression(access.Accept(this)!, context.Index.Accept(this)!);

        throw new NotSupportedException();
    }

    public override IExpression VisitNegateOperationExpression(SPSLParser.NegateOperationExpressionContext context)
    {
        return new NegateOperationExpression(context.Expression.Accept(this)!);
    }

    public override IExpression VisitPostfixUnaryOperationExpression(
        SPSLParser.PostfixUnaryOperationExpressionContext context)
    {
        return new UnaryOperationExpression((IAssignableExpression)context.Expression.Accept(this)!,
            context.Operator.Text, true);
    }

    public override IExpression VisitPrefixUnaryOperationExpression(
        SPSLParser.PrefixUnaryOperationExpressionContext context)
    {
        return new UnaryOperationExpression((IAssignableExpression)context.Expression.Accept(this)!,
            context.Operator.Text, false);
    }

    public override IExpression VisitSignedExpression(SPSLParser.SignedExpressionContext context)
    {
        return new SignedExpression(context.Operator.Text, context.Expression.Accept(this)!);
    }

    public override IExpression VisitBinaryOperationExpression(SPSLParser.BinaryOperationExpressionContext context)
    {
        return new BinaryOperationExpression(context.Left.Accept(this)!, context.Operator.Text,
            context.Right.Accept(this)!);
    }

    public override IExpression VisitTernaryOperationExpression(SPSLParser.TernaryOperationExpressionContext context)
    {
        return new TernaryOperationExpression(context.Condition.Accept(this)!, context.WhenTrue.Accept(this)!,
            context.WhenFalse.Accept(this)!);
    }

    public override IExpression VisitAssignmentExpression(SPSLParser.AssignmentExpressionContext context)
    {
        return new AssignmentExpression((IAssignableExpression)context.Left.Accept(this)!, context.Operator.Text,
            context.Right.Accept(this)!);
    }

    public override IExpression VisitCastExpression(SPSLParser.CastExpressionContext context)
    {
        return new CastExpression(context.Type.Accept(new DataTypeVisitor())!, context.Expression.Accept(this)!);
    }

    public override IExpression VisitNewInstanceExpression(SPSLParser.NewInstanceExpressionContext context)
    {
        OrderedSet<InvocationParameter> parameters = new();
        if (context.Parameters != null)
        {
            parameters = new OrderedSet<InvocationParameter>();
            foreach (SPSLParser.ExpressionStatementContext expression in context.Parameters.expressionStatement())
                parameters.Add(new InvocationParameter(expression.Accept(this)!));
        }

        return new NewInstanceExpression(context.Type.Accept(new DataTypeVisitor())!, parameters);
    }
}
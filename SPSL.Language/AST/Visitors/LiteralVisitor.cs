using Antlr4.Runtime.Tree;

namespace SPSL.Language.AST.Visitors;

public class LiteralVisitor : SPSLBaseVisitor<ILiteral?>
{
    protected override ILiteral? DefaultResult => null;

    protected override ILiteral? AggregateResult(ILiteral? aggregate, ILiteral? nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, ILiteral? currentResult)
    {
        return node is SPSLParser.PrimitiveExpressionContext or SPSLParser.PrimitiveConstantExpressionContext;
    }

    public override ILiteral VisitPrimitiveExpression(SPSLParser.PrimitiveExpressionContext context)
    {
        return context.Literal.Type switch
        {
            SPSLParser.BoolLiteral => new BoolLiteral(bool.Parse(context.Literal.Text)),
            SPSLParser.DoubleLiteral => new DoubleLiteral(double.Parse(context.Literal.Text.TrimEnd('d', 'D'))),
            SPSLParser.FloatLiteral => new FloatLiteral(float.Parse(context.Literal.Text.TrimEnd('f', 'F'))),
            SPSLParser.IntegerLiteral => new IntegerLiteral(int.Parse(context.Literal.Text)),
            SPSLParser.UnsignedIntegerLiteral => new UnsignedIntegerLiteral(
                uint.Parse(context.Literal.Text.TrimEnd('u', 'U'))),
            SPSLParser.StringLiteral => new StringLiteral(context.Literal.Text),
            _ => throw new NotSupportedException(),
        };
    }

    public override ILiteral VisitPrimitiveConstantExpression(SPSLParser.PrimitiveConstantExpressionContext context)
    {
        return context.primitiveExpression().Accept(this)!;
    }
}
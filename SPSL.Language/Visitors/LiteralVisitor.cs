using System.Globalization;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

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
        var isHex = context.Literal.Text.StartsWith("0x", true, CultureInfo.InvariantCulture);
        var isOctal = !isHex && context.Literal.Text.StartsWith("0", true, CultureInfo.InvariantCulture) && context.Literal.Text.Length > 1;
        var a = 0777;
        return context.Literal.Type switch
        {
            SPSLParser.BoolLiteral => new BoolLiteral(bool.Parse(context.Literal.Text)),
            SPSLParser.DoubleLiteral => new DoubleLiteral(double.Parse(context.Literal.Text.TrimEnd('d', 'D'))),
            SPSLParser.FloatLiteral => new FloatLiteral(float.Parse(context.Literal.Text.TrimEnd('f', 'F'))),
            SPSLParser.IntegerLiteral => new IntegerLiteral
            (
                Convert.ToInt32
                (
                    isHex ? context.Literal.Text[2..] : isOctal ? context.Literal.Text[1..] : context.Literal.Text,
                    isHex ? 16 : isOctal? 8 : 10
                )
            )
            {
                IsHexConstant = isHex,
                IsOctalConstant = isOctal,
            },
            SPSLParser.UnsignedIntegerLiteral => new UnsignedIntegerLiteral(uint.Parse(context.Literal.Text.TrimEnd('u', 'U'))),
            SPSLParser.StringLiteral => new StringLiteral(context.Literal.Text),
            _ => throw new NotSupportedException(),
        };
    }

    public override ILiteral VisitPrimitiveConstantExpression(SPSLParser.PrimitiveConstantExpressionContext context)
    {
        return context.primitiveExpression().Accept(this)!;
    }
}
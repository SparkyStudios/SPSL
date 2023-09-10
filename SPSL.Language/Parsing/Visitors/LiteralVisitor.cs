using System.Globalization;
using Antlr4.Runtime.Tree;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;

namespace SPSL.Language.Parsing.Visitors;

public class LiteralVisitor : SPSLBaseVisitor<ILiteral?>
{
    private readonly string _fileSource;

    public LiteralVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

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
        bool isHex = context.Literal.Text.StartsWith("0x", true, CultureInfo.InvariantCulture);
        bool isOctal = !isHex && context.Literal.Text.StartsWith("0", true, CultureInfo.InvariantCulture) &&
                       context.Literal.Text.Length > 1;

        return context.Literal.Type switch
        {
            SPSLParser.BoolLiteral => new BoolLiteral(bool.Parse(context.Literal.Text))
            {
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            SPSLParser.DoubleLiteral => new DoubleLiteral(double.Parse(context.Literal.Text.TrimEnd('d', 'D')))
            {
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            SPSLParser.FloatLiteral => new FloatLiteral(float.Parse(context.Literal.Text.TrimEnd('f', 'F')))
            {
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            SPSLParser.IntegerLiteral => new IntegerLiteral
            (
                Convert.ToInt32
                (
                    isHex ? context.Literal.Text[2..] : isOctal ? context.Literal.Text[1..] : context.Literal.Text,
                    isHex ? 16 : isOctal ? 8 : 10
                )
            )
            {
                IsHexConstant = isHex,
                IsOctalConstant = isOctal,
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            SPSLParser.UnsignedIntegerLiteral => new UnsignedIntegerLiteral
            (
                uint.Parse(context.Literal.Text.TrimEnd('u', 'U'))
            )
            {
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            SPSLParser.StringLiteral => new StringLiteral(context.Literal.Text)
            {
                Start = context.Literal.StartIndex,
                End = context.Literal.StopIndex,
                Source = _fileSource
            },
            _ => throw new NotSupportedException(),
        };
    }

    public override ILiteral VisitPrimitiveConstantExpression(SPSLParser.PrimitiveConstantExpressionContext context)
    {
        return context.primitiveExpression().Accept(this)!;
    }
}
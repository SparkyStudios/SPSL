using Antlr4.Runtime.Tree;

namespace SPSL.Language.AST.Visitors;

public class IBlockVisitor : SPSLBaseVisitor<IBlock?>
{
    protected override IBlock? DefaultResult => null;

    protected override IBlock? AggregateResult(IBlock? aggregate, IBlock? nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IBlock? currentResult)
    {
        return node is SPSLParser.StatementBlockContext;
    }
}
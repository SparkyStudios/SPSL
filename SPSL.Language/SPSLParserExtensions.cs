using SPSL.Language.AST;
using SPSL.Language.Visitors;

namespace SPSL.Language;

public static class SPSLParserExtensions
{
    internal static Annotation ToAnnotation(this SPSLParser.AnnotationContext context)
    {
        return new()
        {
            Name = context.Name.Text,
            Arguments = new(context.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
    }
}
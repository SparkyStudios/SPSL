using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace SPSL.Language.AST.Visitors;

public class ShaderMemberVisitor : SPSLBaseVisitor<IShaderMember?>
{
    protected override IShaderMember? DefaultResult => null;

    protected override bool ShouldVisitNextChild(IRuleNode node, IShaderMember? currentResult)
        => node is SPSLParser.ShaderMemberContext or SPSLParser.BufferDefinitionContext
            or SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext;

    public override IShaderMember VisitBufferDefinition([NotNull] SPSLParser.BufferDefinitionContext context)
    {
        Buffer buffer = new Buffer
        (
            context.Name.Text,
            context.bufferComponent().Select(component => ASTVisitor.ParseBufferComponent(component))
        );

        foreach (SPSLParser.AnnotationContext annotation in context.annotation())
        {
            buffer.Annotations.Add
            (
                new Annotation
                {
                    Name = annotation.Name.Text,
                    Arguments = new(annotation.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
                }
            );
        }

        return buffer;
    }

    public override IShaderMember? VisitStruct(SPSLParser.StructContext context)
    {
        return context.Accept(new TypeVisitor())!;
    }

    public override IShaderMember? VisitEnum(SPSLParser.EnumContext context)
    {
        return context.Accept(new TypeVisitor())!;
    }
}

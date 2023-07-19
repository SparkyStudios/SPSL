using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using Stream = SPSL.Language.AST.Stream;

namespace SPSL.Language.Visitors;

public class ShaderMemberVisitor : SPSLBaseVisitor<IShaderMember?>
{
    protected override IShaderMember? DefaultResult => null;

    private static uint StreamCount = 0;

    private static BufferStorage ParseBufferStorage(string storage)
    {
        return storage switch
        {
            "coherent" => BufferStorage.Coherent,
            _ => BufferStorage.Undefined
        };
    }

    private static BufferAccess ParseBufferAccess(string access)
    {
        return access switch
        {
            "readonly" => BufferAccess.ReadOnly,
            "writeonly" => BufferAccess.WriteOnly,
            "readwrite" => BufferAccess.ReadWrite,
            "constant" => BufferAccess.Constant,
            _ => BufferAccess.ReadOnly
        };
    }

    private static StreamProperty ParseStreamProperty(SPSLParser.StreamPropertyContext context)
    {
        SPSLParser.BufferComponentContext bufferComponentContext = context.bufferComponent();

        StreamProperty property = new(
            bufferComponentContext.Type.Accept(new DataTypeVisitor()),
            bufferComponentContext.Name.Text,
            context.Flow.Text switch
            {
                "input" => StreamPropertyFlow.Input,
                "transient" => StreamPropertyFlow.Transient,
                "output" => StreamPropertyFlow.Output,
                _ => throw new ArgumentOutOfRangeException(nameof(context.Flow), context.Flow.Text, "Invalid stream property flow.")
            }
        );

        foreach (SPSLParser.AnnotationContext annotation in context.annotation())
        {
            property.Annotations.Add
            (
                new Annotation
                {
                    Name = annotation.Name.Text,
                    Arguments = new(annotation.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
                }
            );
        }

        return property;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IShaderMember? currentResult)
        => node is SPSLParser.ShaderMemberContext or SPSLParser.BufferDefinitionContext
            or SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext;

    public override IShaderMember VisitInPlaceStructuredBufferDefinition([NotNull] SPSLParser.InPlaceStructuredBufferDefinitionContext context)
    {
        StructuredBuffer buffer = new
        (
            context.Name.Text,
            context.bufferComponent().Select(component => ASTVisitor.ParseBufferComponent(component))
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : ParseBufferStorage(context.Storage.Text),
            Access = ParseBufferAccess(context.Access.Text)
        };

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

    public override IShaderMember VisitTypedBufferDefinition([NotNull] SPSLParser.TypedBufferDefinitionContext context)
    {
        TypedBuffer buffer = new
        (
            context.Name.Text,
            context.dataType().Accept(new DataTypeVisitor())
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : ParseBufferStorage(context.Storage.Text),
            Access = ParseBufferAccess(context.Access.Text)
        };

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
        return context.Accept(new TypeVisitor());
    }

    public override IShaderMember? VisitEnum(SPSLParser.EnumContext context)
    {
        return context.Accept(new TypeVisitor());
    }

    public override IShaderMember? VisitStream(SPSLParser.StreamContext context)
    {
        return new Stream
        (
            $"@stream{++StreamCount}",
            context.streamProperty().Select(property => ParseStreamProperty(property))
        );
    }
}
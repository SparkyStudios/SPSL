using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using Stream = SPSL.Language.AST.Stream;

namespace SPSL.Language.Visitors;

public class ShaderMemberVisitor : SPSLBaseVisitor<IShaderMember?>
{
    private static uint _streamCount = 0;

    protected override IShaderMember? DefaultResult => null;

    private readonly string _fileSource;

    public ShaderMemberVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

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

    private StreamProperty ParseStreamProperty(SPSLParser.StreamPropertyContext context)
    {
        SPSLParser.BufferComponentContext bufferComponentContext = context.bufferComponent();

        StreamProperty property = new
        (
            bufferComponentContext.Type.Accept(new DataTypeVisitor(_fileSource)),
            bufferComponentContext.Name.Text,
            context.Flow.Text switch
            {
                "input" => StreamPropertyFlow.Input,
                "transient" => StreamPropertyFlow.Transient,
                "output" => StreamPropertyFlow.Output,
                _ => throw new ArgumentOutOfRangeException(nameof(context.Flow), context.Flow.Text,
                    "Invalid stream property flow.")
            }
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
        };

        property.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return property;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IShaderMember? currentResult)
        => node is SPSLParser.ShaderMemberContext or SPSLParser.BufferDefinitionContext
            or SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext;

    public override IShaderMember VisitInPlaceStructuredBufferDefinition(
        [NotNull] SPSLParser.InPlaceStructuredBufferDefinitionContext context)
    {
        StructuredBuffer buffer = new
        (
            context.Name.Text,
            context.bufferComponent().Select(c => c.ToTypeProperty(_fileSource))
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : ParseBufferStorage(context.Storage.Text),
            Access = ParseBufferAccess(context.Access.Text),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        buffer.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return buffer;
    }

    public override IShaderMember VisitTypedBufferDefinition([NotNull] SPSLParser.TypedBufferDefinitionContext context)
    {
        TypedBuffer buffer = new
        (
            context.Name.Text,
            context.dataType().Accept(new DataTypeVisitor(_fileSource))
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : ParseBufferStorage(context.Storage.Text),
            Access = ParseBufferAccess(context.Access.Text),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        buffer.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return buffer;
    }

    public override IShaderMember? VisitStruct(SPSLParser.StructContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IShaderMember? VisitEnum(SPSLParser.EnumContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IShaderMember? VisitStream(SPSLParser.StreamContext context)
    {
        return new Stream
        (
            $"@stream{++_streamCount}",
            context.streamProperty().Select(ParseStreamProperty)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IShaderMember? VisitGlobalVariable(SPSLParser.GlobalVariableContext context)
    {
        return new GlobalVariable
        (
            context.IsStatic,
            context.Type.Accept(new DataTypeVisitor(_fileSource)),
            context.Definition.Expression.Accept(new ExpressionVisitor(_fileSource))!
        )
        {
            Name = context.Definition.Identifier.Identifier.Text,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }
}
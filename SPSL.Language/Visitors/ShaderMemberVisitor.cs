using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Utils;
using static SPSL.Language.SPSLParser;
using Stream = SPSL.Language.AST.Stream;

namespace SPSL.Language.Visitors;

public class ShaderMemberVisitor : SPSLBaseVisitor<IShaderMember?>
{
    private static uint _streamCount;

    protected override IShaderMember? DefaultResult => null;

    private readonly string _fileSource;

    public ShaderMemberVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    private StreamProperty ParseStreamProperty(StreamPropertyContext context)
    {
        BufferComponentContext bufferComponentContext = context.bufferComponent();

        StreamProperty property = new
        (
            bufferComponentContext.Type.Accept(new DataTypeVisitor(_fileSource)),
            bufferComponentContext.Name.ToIdentifier(_fileSource),
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
        => node is ShaderMemberContext or GlobalVariableContext or BufferDefinitionContext
            or TypeContext or StructContext or EnumContext or ShaderFunctionContext;

    public override IShaderMember VisitInPlaceStructuredBufferDefinition(
        [NotNull] InPlaceStructuredBufferDefinitionContext context)
    {
        StructuredBuffer buffer = new
        (
            context.Name.ToIdentifier(_fileSource),
            context.bufferComponent().Select(c => c.ToTypeProperty(_fileSource))
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : context.Storage.Text.ToBufferStorage(),
            Access = context.Access.Text.ToBufferAccess(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        buffer.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return buffer;
    }

    public override IShaderMember VisitTypedBufferDefinition([NotNull] TypedBufferDefinitionContext context)
    {
        TypedBuffer buffer = new
        (
            context.Name.ToIdentifier(_fileSource),
            context.dataType().Accept(new DataTypeVisitor(_fileSource))
        )
        {
            Storage = context.Storage == null ? BufferStorage.Undefined : context.Storage.Text.ToBufferStorage(),
            Access = context.Access.Text.ToBufferAccess(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        buffer.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return buffer;
    }

    public override IShaderMember? VisitStruct(StructContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IShaderMember? VisitEnum(EnumContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IShaderMember VisitStream(StreamContext context)
    {
        return new Stream
        (
            new() { Value = $"stream@{++_streamCount}" },
            context.streamProperty().Select(ParseStreamProperty)
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    public override IShaderMember VisitGlobalVariable(GlobalVariableContext context)
    {
        return new GlobalVariable
        (
            context.IsStatic,
            context.Type.Accept(new DataTypeVisitor(_fileSource)),
            context.Definition.Expression.Accept(new ExpressionVisitor(_fileSource))!
        )
        {
            Name = context.Definition.Identifier.Identifier.ToIdentifier(_fileSource),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };
    }

    public override IShaderMember VisitBasicShaderFunction(BasicShaderFunctionContext context)
    {
        var function = new ShaderFunction(context.Function.ToFunction(_fileSource))
        {
            IsOverride = context.IsOverride,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return function;
    }

    public override IShaderMember VisitShaderConstructorFunction(ShaderConstructorFunctionContext context)
    {
        var function = new ShaderFunction
        (
            new
            (
                new
                (
                    new PrimitiveDataType(PrimitiveDataTypeKind.Void),
                    context.Name.ToIdentifier(_fileSource),
                    new()
                ),
                context.Body.ToStatementBlock(_fileSource)
            )
        )
        {
            IsOverride = false,
            IsConstructor = true,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return function;
    }
}
using SPSL.Language.AST;
using SPSL.Language.Core;

namespace SPSL.Language.Visitors;

public class ShaderFunctionVisitor : SPSLBaseVisitor<ShaderFunction?>
{
    protected override ShaderFunction? DefaultResult => null;

    private readonly string _fileSource;

    public ShaderFunctionVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    public override ShaderFunction VisitBasicShaderFunction(SPSLParser.BasicShaderFunctionContext context)
    {
        var function = new ShaderFunction(context.Function.ToFunction(_fileSource))
        {
            IsOverride = context.IsOverride,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return function;
    }

    public override ShaderFunction VisitShaderConstructorFunction(SPSLParser.ShaderConstructorFunctionContext context)
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
            Source = _fileSource
        };

        function.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return function;
    }
}
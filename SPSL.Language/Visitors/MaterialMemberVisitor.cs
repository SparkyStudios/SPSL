using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Utils;

namespace SPSL.Language.Visitors;

public class MaterialMemberVisitor : SPSLBaseVisitor<IMaterialMember?>
{
    protected override IMaterialMember? DefaultResult => null;

    private readonly string _fileSource;

    public MaterialMemberVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IMaterialMember? currentResult)
        => node is SPSLParser.MaterialMemberContext or SPSLParser.MaterialParamsContext
            or SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext
            or SPSLParser.MaterialStateContext or SPSLParser.MaterialStateBlockContext
            or SPSLParser.MaterialStateValueContext
            or SPSLParser.MaterialShaderUsageContext;

    public override IMaterialMember? VisitMaterialParams([NotNull] SPSLParser.MaterialParamsContext context)
    {
        MaterialParameterGroup materialParams = new(context.Name.Text)
        {
            IsPartial = context.IsPartial,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        foreach (var param in context.materialParamsComponent())
        {
            MaterialParameter materialParam;

            if (param is SPSLParser.MaterialValueParameterContext v)
            {
                materialParam = new(v.Type.Accept(new DataTypeVisitor(_fileSource)), v.Name.Text,
                    MaterialParameterType.Value)
                {
                    DefaultValue = v.DefaultValue?.Accept(new ExpressionVisitor(_fileSource)),
                    Start = v.Start.StartIndex,
                    End = v.Stop.StopIndex,
                    Source = _fileSource
                };

                materialParam.Annotations.AddRange(v.annotation().Select(a => a.ToAnnotation(_fileSource)));
            }
            else if (param is SPSLParser.MaterialPermutationParameterContext p)
            {
                var permutation = p.permutationVariable().ToPermutationVariable(_fileSource);

                materialParam = new
                (
                    permutation.Type switch
                    {
                        PermutationVariableType.Bool => new PrimitiveDataType(PrimitiveDataTypeKind.Boolean),
                        PermutationVariableType.Integer =>
                            new PrimitiveDataType(PrimitiveDataTypeKind.Integer),
                        PermutationVariableType.Enum => new UserDefinedDataType(new(permutation.Name)),
                        _ => throw new ArgumentException("Invalid permutation variable type"),
                    },
                    permutation.Name,
                    MaterialParameterType.Permutation
                )
                {
                    DefaultValue = permutation.Initializer,
                    Start = p.Start.StartIndex,
                    End = p.Stop.StopIndex,
                    Source = _fileSource
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            materialParams.Children.Add(materialParam);
        }

        materialParams.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return materialParams;
    }

    public override IMaterialMember? VisitMaterialStateValue([NotNull] SPSLParser.MaterialStateValueContext context)
    {
        return new MaterialState(context.Name.Text)
        {
            Value = context.Value.Text,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IMaterialMember? VisitMaterialStateBlock([NotNull] SPSLParser.MaterialStateBlockContext context)
    {
        return new MaterialState
        (
            context.Name.Text,
            context.materialStateComponent().Select
            (
                c => new MaterialStateComponent
                (
                    c.Name.Text,
                    c.initializationExpression().Accept(new ExpressionVisitor(_fileSource))!
                )
                {
                    Start = c.Start.StartIndex,
                    End = c.Stop.StopIndex,
                    Source = _fileSource
                }
            )
        )
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IMaterialMember? VisitStruct(SPSLParser.StructContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IMaterialMember? VisitEnum(SPSLParser.EnumContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IMaterialMember? VisitSimpleMaterialShaderUsage(SPSLParser.SimpleMaterialShaderUsageContext context)
    {
        return new MaterialShader(context.Definition.Stage.Text)
        {
            ReferencedShader = ASTVisitor.ParseNamespacedTypeName(context.Definition.Name),
            Stage = context.Definition.Stage.Text.ToShaderStage(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IMaterialMember? VisitCustomizedMaterialShaderUsage(
        SPSLParser.CustomizedMaterialShaderUsageContext context)
    {
        MaterialShader shader = new(context.Name?.Text ?? context.Definition.Stage.Text)
        {
            ReferencedShader = ASTVisitor.ParseNamespacedTypeName(context.Definition.Name),
            Stage = context.Definition.Stage.Text.ToShaderStage(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        shader.Children.AddRange
        (
            context.shaderFunction().Select(f => f.Accept(new ShaderFunctionVisitor(_fileSource))!)
        );
        shader.ImportedShaderFragments.AddRange
        (
            context.useFragmentDirective().Select(c => ASTVisitor.ParseNamespacedTypeName(c.Name))
        );

        return shader;
    }
}
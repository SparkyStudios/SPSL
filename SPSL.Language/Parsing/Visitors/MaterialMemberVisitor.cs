using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Parsing.Utils;
using static SPSL.Language.Core.SPSLParser;

namespace SPSL.Language.Parsing.Visitors;

public class MaterialMemberVisitor : SPSLBaseVisitor<IMaterialMember?>
{
    protected override IMaterialMember? DefaultResult => null;

    private readonly string _fileSource;

    public MaterialMemberVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IMaterialMember? currentResult)
        => node is MaterialMemberContext or MaterialParamsContext
            or TypeContext or StructContext or EnumContext
            or MaterialStateContext or MaterialStateBlockContext
            or MaterialStateValueContext
            or MaterialShaderUsageContext or MaterialVariantContext;

    public override IMaterialMember VisitMaterialParams([NotNull] MaterialParamsContext context)
    {
        MaterialParameterGroup materialParams = new(context.Name.ToIdentifier(_fileSource))
        {
            IsPartial = context.IsPartial,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        foreach (var param in context.materialParamsComponent())
        {
            MaterialParameter materialParam;

            switch (param)
            {
                case MaterialValueParameterContext v:
                {
                    materialParam = new
                    (
                        v.Type.Accept(new DataTypeVisitor(_fileSource)),
                        v.Name.ToIdentifier(_fileSource),
                        MaterialParameterType.Value
                    )
                    {
                        DefaultValue = v.DefaultValue?.Accept(new ExpressionVisitor(_fileSource)),
                        Start = v.Start.StartIndex,
                        End = v.Stop.StopIndex,
                        Source = _fileSource
                    };

                    materialParam.Annotations.AddRange(v.annotation().Select(a => a.ToAnnotation(_fileSource)));
                    break;
                }
                case MaterialPermutationParameterContext p:
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
                    break;
                }
                default:
                    throw new NotImplementedException();
            }

            materialParams.Children.Add(materialParam);
        }

        materialParams.Annotations.AddRange(context.annotation().Select(a => a.ToAnnotation(_fileSource)));
        return materialParams;
    }

    public override IMaterialMember VisitMaterialStateValue([NotNull] MaterialStateValueContext context)
    {
        return new MaterialState(context.Name.ToIdentifier(_fileSource))
        {
            Value = context.Value.Text,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IMaterialMember VisitMaterialStateBlock([NotNull] MaterialStateBlockContext context)
    {
        return new MaterialState
        (
            context.Name.ToIdentifier(_fileSource),
            context.materialStateComponent().Select
            (
                c => new MaterialStateComponent
                (
                    c.Name.ToIdentifier(_fileSource),
                    c.Value.ToIdentifier(_fileSource)
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

    public override IMaterialMember? VisitStruct([NotNull] StructContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IMaterialMember? VisitEnum([NotNull] EnumContext context)
    {
        return context.Accept(new TypeVisitor(_fileSource));
    }

    public override IMaterialMember VisitSimpleMaterialShaderUsage([NotNull] SimpleMaterialShaderUsageContext context)
    {
        return new MaterialShader(context.Definition.Stage.ToIdentifier(_fileSource))
        {
            ReferencedShader = context.Definition.Name.ToNamespaceReference(_fileSource),
            Stage = context.Definition.Stage.ToShaderStage(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IMaterialMember VisitCustomizedMaterialShaderUsage(
        [NotNull] CustomizedMaterialShaderUsageContext context)
    {
        MaterialShader shader = new
        (
            context.Name?.ToIdentifier(_fileSource) ?? context.Definition.Stage.ToIdentifier(_fileSource)
        )
        {
            ReferencedShader = context.Definition.Name.ToNamespaceReference(_fileSource),
            Stage = context.Definition.Stage.ToShaderStage(),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        shader.Children.AddRange
        (
            context.shaderFunction().Select(f => f.Accept(new ShaderMemberVisitor(_fileSource))!)
        );

        shader.ImportedShaderFragments.AddRange
        (
            context.useFragmentDirective().Select(c => c.Name.ToNamespaceReference(_fileSource))
        );

        return shader;
    }

    public override IMaterialMember VisitMaterialVariant([NotNull] MaterialVariantContext context)
    {
        return new MaterialVariant(context.Name.ToIdentifier(_fileSource))
        {
            PermutationValues = new
            (
                context._Values.Select
                (
                    v =>
                        new BinaryOperationExpression
                        (
                            new BasicExpression(v.Name.ToIdentifier(_fileSource)),
                            Op.Assignment,
                            v.Value.Accept(new ExpressionVisitor(_fileSource))
                        )
                )
            ),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }
}
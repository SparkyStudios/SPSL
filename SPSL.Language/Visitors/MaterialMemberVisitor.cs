using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class MaterialMemberVisitor : SPSLBaseVisitor<IMaterialMember?>
{
    protected override IMaterialMember? DefaultResult => null;

    protected override bool ShouldVisitNextChild(IRuleNode node, IMaterialMember? currentResult)
        => node is SPSLParser.MaterialMemberContext or SPSLParser.MaterialParamsContext
            or SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext
            or SPSLParser.MaterialStateContext or SPSLParser.MaterialStateBlockContext or SPSLParser.MaterialStateValueContext
            or SPSLParser.MaterialShaderUsageContext;

    public override IMaterialMember? VisitMaterialParams([NotNull] SPSLParser.MaterialParamsContext context)
    {
        MaterialParameterGroup materialParams = new(context.Name.Text)
        {
            IsPartial = context.IsPartial
        };

        foreach (var param in context.materialParamsComponent())
        {
            MaterialParameter materialParam;

            if (param is SPSLParser.MaterialValueParameterContext v)
            {
                materialParam = new(v.Type.Accept(new DataTypeVisitor()), v.Name.Text, MaterialParameterType.Value)
                {
                    DefaultValue = v.DefaultValue?.Accept(new ExpressionVisitor()),
                };

                materialParam.Annotations.AddRange(v.annotation().Select(a => new Annotation
                {
                    Name = a.Name.Text,
                    Arguments = new(a.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
                }));
            }
            else if (param is SPSLParser.MaterialPermutationParameterContext p)
            {
                PermutationVariable permutation = ASTVisitor.ParsePermutationVariable(p.permutationVariable());

                materialParam = new
                (
                    permutation.Type switch
                    {
                        PermutationVariable.VariableType.Bool => new PrimitiveDataType(PrimitiveDataTypeKind.Boolean),
                        PermutationVariable.VariableType.Integer => new PrimitiveDataType(PrimitiveDataTypeKind.Integer),
                        PermutationVariable.VariableType.Enum => new UserDefinedDataType(new(permutation.Name)),
                        _ => throw new ArgumentException("Invalid permutation variable type"),
                    },
                    permutation.Name,
                    MaterialParameterType.Permutation
                )
                {
                    DefaultValue = permutation.Initializer
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            materialParams.Children.Add(materialParam);
        }

        materialParams.Annotations.AddRange(context.annotation().Select(a => new Annotation
        {
            Name = a.Name.Text,
            Arguments = new(a.constantExpression().Select(e => e.Accept(new ExpressionVisitor())!)),
        }));

        return materialParams;
    }

    public override IMaterialMember? VisitMaterialStateValue([NotNull] SPSLParser.MaterialStateValueContext context)
    {
        return new MaterialState(context.Name.Text)
        {
            Value = context.Value.Text,
        };
    }

    public override IMaterialMember? VisitMaterialStateBlock([NotNull] SPSLParser.MaterialStateBlockContext context)
    {
        MaterialState materialStateBlock = new(context.Name.Text);
        materialStateBlock.Children.AddRange(context.materialStateComponent().Select(c => new MaterialStateComponent(c.Name.Text, c.initializationExpression().Accept(new ExpressionVisitor())!)));

        return materialStateBlock;
    }

    public override IMaterialMember? VisitStruct(SPSLParser.StructContext context)
    {
        return context.Accept(new TypeVisitor());
    }

    public override IMaterialMember? VisitEnum(SPSLParser.EnumContext context)
    {
        return context.Accept(new TypeVisitor());
    }

    public override IMaterialMember? VisitSimpleMaterialShaderUsage(SPSLParser.SimpleMaterialShaderUsageContext context)
    {
        return new MaterialShader(context.Definition.Stage.Text)
        {
            ReferencedShader = ASTVisitor.ParseNamespacedTypeName(context.Definition.Name),
            Stage = ShaderVisitor.GetShaderStage(context.Definition.Stage.Text),
        };
    }

    public override IMaterialMember? VisitCustomizedMaterialShaderUsage(SPSLParser.CustomizedMaterialShaderUsageContext context)
    {
        MaterialShader shader = new(context.Name?.Text ?? context.Definition.Stage.Text)
        {
            ReferencedShader = ASTVisitor.ParseNamespacedTypeName(context.Definition.Name),
            Stage = ShaderVisitor.GetShaderStage(context.Definition.Stage.Text),
        };

        shader.Children.AddRange(context.shaderFunction().Select(c => ASTVisitor.ParseShaderFunction(c)));
        shader.ImportedShaderFragments.AddRange(context.useDirective().Select(c => ASTVisitor.ParseNamespacedTypeName(c.Name)));

        return shader;
    }
}

using Antlr4.Runtime.Tree;
using SPSL.Language.AST;

namespace SPSL.Language.Visitors;

public class DataTypeVisitor : SPSLBaseVisitor<IDataType>
{
    protected override IDataType DefaultResult => new UnknownDataType();

    protected override IDataType AggregateResult(IDataType aggregate, IDataType nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IDataType currentResult)
    {
        switch (node)
        {
            case SPSLParser.DataTypeContext _:
            case SPSLParser.PrimitiveDataTypeContext _:
            case SPSLParser.BuiltinDataTypeContext _:
            case SPSLParser.UserDefinedDataTypeContext _:
                return true;

            // Not parsed
            default:
                return false;
        }
    }

    public override IDataType VisitDataType(SPSLParser.DataTypeContext context)
    {
        IDataType type = context.DataType switch
        {
            DataTypeKind.Primitive => context.primitiveDataType().Accept(this),
            DataTypeKind.BuiltIn => context.builtinDataType().Accept(this),
            DataTypeKind.UserDefined => context.userDefinedDataType().Accept(this),
            _ => DefaultResult,
        };

        type.IsArray = context.IsArray;

        if (context.ArraySize != null)
            type.ArraySize = uint.Parse(context.ArraySize.Text.TrimEnd('u', 'U'));

        return type;
    }

    public override IDataType VisitPrimitiveDataType(SPSLParser.PrimitiveDataTypeContext context)
    {
        PrimitiveDataTypeKind kind = context.Type.Type switch
        {
            SPSLParser.TYPE_VOID => PrimitiveDataTypeKind.Void,
            SPSLParser.TYPE_BOOL => PrimitiveDataTypeKind.Boolean,
            SPSLParser.TYPE_INT => PrimitiveDataTypeKind.Integer,
            SPSLParser.TYPE_UINT => PrimitiveDataTypeKind.UnsignedInteger,
            SPSLParser.TYPE_FLOAT => PrimitiveDataTypeKind.Float,
            SPSLParser.TYPE_DOUBLE => PrimitiveDataTypeKind.Double,
            SPSLParser.TYPE_STRING => PrimitiveDataTypeKind.String,
            _ => throw new NotSupportedException("The given primitive type is not supported."),
        };

        return new PrimitiveDataType(kind);
    }

    public override IDataType VisitBuiltinDataType(SPSLParser.BuiltinDataTypeContext context)
    {
        BuiltInDataTypeKind kind = context.Type.Type switch
        {
            SPSLParser.TYPE_VECTOR2B => BuiltInDataTypeKind.Vector2b,
            SPSLParser.TYPE_VECTOR2F => BuiltInDataTypeKind.Vector2f,
            SPSLParser.TYPE_VECTOR2I => BuiltInDataTypeKind.Vector2i,
            SPSLParser.TYPE_VECTOR2UI => BuiltInDataTypeKind.Vector2ui,
            SPSLParser.TYPE_VECTOR3B => BuiltInDataTypeKind.Vector3b,
            SPSLParser.TYPE_VECTOR3F => BuiltInDataTypeKind.Vector3f,
            SPSLParser.TYPE_VECTOR3I => BuiltInDataTypeKind.Vector3i,
            SPSLParser.TYPE_VECTOR3UI => BuiltInDataTypeKind.Vector3ui,
            SPSLParser.TYPE_VECTOR4B => BuiltInDataTypeKind.Vector4b,
            SPSLParser.TYPE_VECTOR4F => BuiltInDataTypeKind.Vector4f,
            SPSLParser.TYPE_VECTOR4I => BuiltInDataTypeKind.Vector4i,
            SPSLParser.TYPE_VECTOR4UI => BuiltInDataTypeKind.Vector4ui,
            SPSLParser.TYPE_MATRIX2F => BuiltInDataTypeKind.Matrix2f,
            SPSLParser.TYPE_MATRIX3F => BuiltInDataTypeKind.Matrix3f,
            SPSLParser.TYPE_MATRIX4F => BuiltInDataTypeKind.Matrix4f,
            SPSLParser.TYPE_MATRIX2X3F => BuiltInDataTypeKind.Matrix2x3f,
            SPSLParser.TYPE_MATRIX2X4F => BuiltInDataTypeKind.Matrix2x4f,
            SPSLParser.TYPE_MATRIX3X2F => BuiltInDataTypeKind.Matrix3x2f,
            SPSLParser.TYPE_MATRIX3X4F => BuiltInDataTypeKind.Matrix3x4f,
            SPSLParser.TYPE_MATRIX4X2F => BuiltInDataTypeKind.Matrix4x2f,
            SPSLParser.TYPE_MATRIX4X3F => BuiltInDataTypeKind.Matrix4x3f,
            SPSLParser.TYPE_COLOR3 => BuiltInDataTypeKind.Color3,
            SPSLParser.TYPE_COLOR4 => BuiltInDataTypeKind.Color4,
            SPSLParser.TYPE_SAMPLER => BuiltInDataTypeKind.Sampler,
            SPSLParser.TYPE_TEXTURE1D => BuiltInDataTypeKind.Texture1D,
            SPSLParser.TYPE_TEXTURE1DARRAY => BuiltInDataTypeKind.ArrayTexture1D,
            SPSLParser.TYPE_TEXTURE2D => BuiltInDataTypeKind.Texture2D,
            SPSLParser.TYPE_TEXTURE2DARRAY => BuiltInDataTypeKind.ArrayTexture2D,
            SPSLParser.TYPE_TEXTURE3D => BuiltInDataTypeKind.Texture3D,
            SPSLParser.TYPE_CUBEMAP => BuiltInDataTypeKind.Cubemap,
            SPSLParser.TYPE_CUBEMAPARRAY => BuiltInDataTypeKind.ArrayCubemap,
            _ => throw new NotSupportedException("The given primitive type is not supported."),
        };

        return new BuiltInDataType(kind);
    }

    public override IDataType VisitUserDefinedDataType(SPSLParser.UserDefinedDataTypeContext context)
    {
        return new UserDefinedDataType(new NamespacedReference(context.Type.GetText()));
    }
}

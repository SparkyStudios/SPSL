using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Core;

namespace SPSL.Language.Visitors;

public class DataTypeVisitor : SPSLBaseVisitor<IDataType>
{
    private readonly string _fileSource;

    public DataTypeVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

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

    public override IDataType VisitLanguageDataType(SPSLParser.LanguageDataTypeContext context)
    {
        IDataType type = context.DataType switch
        {
            DataTypeKind.Primitive => context.primitiveDataType().Accept(this),
            DataTypeKind.BuiltIn => context.builtinDataType().Accept(this),
            _ => DefaultResult,
        };

        type.IsArray = context.IsArray;

        if (context.ArraySize != null)
            type.ArraySize = uint.Parse(context.ArraySize.Text.TrimEnd('u', 'U'));

        return type;
    }

    public override IDataType VisitCustomDataType(SPSLParser.CustomDataTypeContext context)
    {
        IDataType type = context.DataType switch
        {
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

        return new PrimitiveDataType(kind)
        {
            Start = context.Start.Line,
            End = context.Stop.Line,
            Source = _fileSource
        };
    }

    public override IDataType VisitBuiltinDataType(SPSLParser.BuiltinDataTypeContext context)
    {
        BuiltInDataTypeKind kind = context.Type.Type switch
        {
            SPSLParser.TYPE_VECTOR2B => BuiltInDataTypeKind.Vector2b,
            SPSLParser.TYPE_VECTOR2F => BuiltInDataTypeKind.Vector2f,
            SPSLParser.TYPE_VECTOR2D => BuiltInDataTypeKind.Vector2d,
            SPSLParser.TYPE_VECTOR2I => BuiltInDataTypeKind.Vector2i,
            SPSLParser.TYPE_VECTOR2UI => BuiltInDataTypeKind.Vector2ui,
            SPSLParser.TYPE_VECTOR3B => BuiltInDataTypeKind.Vector3b,
            SPSLParser.TYPE_VECTOR3F => BuiltInDataTypeKind.Vector3f,
            SPSLParser.TYPE_VECTOR3D => BuiltInDataTypeKind.Vector3d,
            SPSLParser.TYPE_VECTOR3I => BuiltInDataTypeKind.Vector3i,
            SPSLParser.TYPE_VECTOR3UI => BuiltInDataTypeKind.Vector3ui,
            SPSLParser.TYPE_VECTOR4B => BuiltInDataTypeKind.Vector4b,
            SPSLParser.TYPE_VECTOR4F => BuiltInDataTypeKind.Vector4f,
            SPSLParser.TYPE_VECTOR4D => BuiltInDataTypeKind.Vector4d,
            SPSLParser.TYPE_VECTOR4I => BuiltInDataTypeKind.Vector4i,
            SPSLParser.TYPE_VECTOR4UI => BuiltInDataTypeKind.Vector4ui,
            SPSLParser.TYPE_MATRIX2F => BuiltInDataTypeKind.Matrix2f,
            SPSLParser.TYPE_MATRIX2D => BuiltInDataTypeKind.Matrix2d,
            SPSLParser.TYPE_MATRIX3F => BuiltInDataTypeKind.Matrix3f,
            SPSLParser.TYPE_MATRIX3D => BuiltInDataTypeKind.Matrix3d,
            SPSLParser.TYPE_MATRIX4F => BuiltInDataTypeKind.Matrix4f,
            SPSLParser.TYPE_MATRIX4D => BuiltInDataTypeKind.Matrix4d,
            SPSLParser.TYPE_MATRIX2X3F => BuiltInDataTypeKind.Matrix2x3f,
            SPSLParser.TYPE_MATRIX2X3D => BuiltInDataTypeKind.Matrix2x3d,
            SPSLParser.TYPE_MATRIX2X4F => BuiltInDataTypeKind.Matrix2x4f,
            SPSLParser.TYPE_MATRIX2X4D => BuiltInDataTypeKind.Matrix2x4d,
            SPSLParser.TYPE_MATRIX3X2F => BuiltInDataTypeKind.Matrix3x2f,
            SPSLParser.TYPE_MATRIX3X2D => BuiltInDataTypeKind.Matrix3x2d,
            SPSLParser.TYPE_MATRIX3X4F => BuiltInDataTypeKind.Matrix3x4f,
            SPSLParser.TYPE_MATRIX3X4D => BuiltInDataTypeKind.Matrix3x4d,
            SPSLParser.TYPE_MATRIX4X2F => BuiltInDataTypeKind.Matrix4x2f,
            SPSLParser.TYPE_MATRIX4X2D => BuiltInDataTypeKind.Matrix4x2d,
            SPSLParser.TYPE_MATRIX4X3F => BuiltInDataTypeKind.Matrix4x3f,
            SPSLParser.TYPE_MATRIX4X3D => BuiltInDataTypeKind.Matrix4x3d,
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

        return new BuiltInDataType(kind)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }

    public override IDataType VisitUserDefinedDataType(SPSLParser.UserDefinedDataTypeContext context)
    {
        return new UserDefinedDataType(context.Type.ToNamespaceReference(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };
    }
}
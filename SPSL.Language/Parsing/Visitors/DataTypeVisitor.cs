using Antlr4.Runtime.Tree;
using SPSL.Language.Core;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;
using SPSL.Language.Parsing.Utils;
using static SPSL.Language.Core.SPSLParser;

namespace SPSL.Language.Parsing.Visitors;

public class DataTypeVisitor : SPSLBaseVisitor<IDataType>
{
    private readonly string _fileSource;

    public DataTypeVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

    protected override IDataType DefaultResult => new UnknownDataType()
    {
        Source = _fileSource
    };

    protected override IDataType AggregateResult(IDataType aggregate, IDataType nextResult)
    {
        // No aggregation
        return nextResult;
    }

    protected override bool ShouldVisitNextChild(IRuleNode node, IDataType currentResult)
    {
        return node is DataTypeContext or PrimitiveDataTypeContext or BuiltinDataTypeContext
            or UserDefinedDataTypeContext;
    }

    public override IDataType VisitLanguageDataType(LanguageDataTypeContext context)
    {
        return context.DataType switch
        {
            DataTypeKind.Primitive => context.primitiveDataType().Accept(this),
            DataTypeKind.BuiltIn => context.builtinDataType().Accept(this),
            _ => DefaultResult,
        };
    }

    public override IDataType VisitCustomDataType(CustomDataTypeContext context)
    {
        return context.DataType switch
        {
            DataTypeKind.UserDefined => context.userDefinedDataType().Accept(this),
            _ => DefaultResult,
        };
    }

    public override IDataType VisitPrimitiveDataType(PrimitiveDataTypeContext context)
    {
        PrimitiveDataTypeKind kind = context.Type.Type switch
        {
            TYPE_VOID => PrimitiveDataTypeKind.Void,
            TYPE_BOOL => PrimitiveDataTypeKind.Boolean,
            TYPE_INT => PrimitiveDataTypeKind.Integer,
            TYPE_UINT => PrimitiveDataTypeKind.UnsignedInteger,
            TYPE_FLOAT => PrimitiveDataTypeKind.Float,
            TYPE_DOUBLE => PrimitiveDataTypeKind.Double,
            TYPE_STRING => PrimitiveDataTypeKind.String,
            _ => throw new NotSupportedException("The given primitive type is not supported."),
        };

        var languageType = context.Parent as LanguageDataTypeContext;
        bool isLanguageType = languageType is not null;

        return new PrimitiveDataType(kind)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            IsArray = isLanguageType && languageType!.IsArray,
            ArraySize = isLanguageType && languageType!.ArraySize != null
                ? uint.Parse(languageType!.ArraySize.Text.TrimEnd('u', 'U'))
                : null
        };
    }

    public override IDataType VisitBuiltinDataType(BuiltinDataTypeContext context)
    {
        BuiltInDataTypeKind kind = context.Type.Type switch
        {
            TYPE_VECTOR2B => BuiltInDataTypeKind.Vector2b,
            TYPE_VECTOR2F => BuiltInDataTypeKind.Vector2f,
            TYPE_VECTOR2D => BuiltInDataTypeKind.Vector2d,
            TYPE_VECTOR2I => BuiltInDataTypeKind.Vector2i,
            TYPE_VECTOR2UI => BuiltInDataTypeKind.Vector2ui,
            TYPE_VECTOR3B => BuiltInDataTypeKind.Vector3b,
            TYPE_VECTOR3F => BuiltInDataTypeKind.Vector3f,
            TYPE_VECTOR3D => BuiltInDataTypeKind.Vector3d,
            TYPE_VECTOR3I => BuiltInDataTypeKind.Vector3i,
            TYPE_VECTOR3UI => BuiltInDataTypeKind.Vector3ui,
            TYPE_VECTOR4B => BuiltInDataTypeKind.Vector4b,
            TYPE_VECTOR4F => BuiltInDataTypeKind.Vector4f,
            TYPE_VECTOR4D => BuiltInDataTypeKind.Vector4d,
            TYPE_VECTOR4I => BuiltInDataTypeKind.Vector4i,
            TYPE_VECTOR4UI => BuiltInDataTypeKind.Vector4ui,
            TYPE_MATRIX2F => BuiltInDataTypeKind.Matrix2f,
            TYPE_MATRIX2D => BuiltInDataTypeKind.Matrix2d,
            TYPE_MATRIX3F => BuiltInDataTypeKind.Matrix3f,
            TYPE_MATRIX3D => BuiltInDataTypeKind.Matrix3d,
            TYPE_MATRIX4F => BuiltInDataTypeKind.Matrix4f,
            TYPE_MATRIX4D => BuiltInDataTypeKind.Matrix4d,
            TYPE_MATRIX2X3F => BuiltInDataTypeKind.Matrix2x3f,
            TYPE_MATRIX2X3D => BuiltInDataTypeKind.Matrix2x3d,
            TYPE_MATRIX2X4F => BuiltInDataTypeKind.Matrix2x4f,
            TYPE_MATRIX2X4D => BuiltInDataTypeKind.Matrix2x4d,
            TYPE_MATRIX3X2F => BuiltInDataTypeKind.Matrix3x2f,
            TYPE_MATRIX3X2D => BuiltInDataTypeKind.Matrix3x2d,
            TYPE_MATRIX3X4F => BuiltInDataTypeKind.Matrix3x4f,
            TYPE_MATRIX3X4D => BuiltInDataTypeKind.Matrix3x4d,
            TYPE_MATRIX4X2F => BuiltInDataTypeKind.Matrix4x2f,
            TYPE_MATRIX4X2D => BuiltInDataTypeKind.Matrix4x2d,
            TYPE_MATRIX4X3F => BuiltInDataTypeKind.Matrix4x3f,
            TYPE_MATRIX4X3D => BuiltInDataTypeKind.Matrix4x3d,
            TYPE_COLOR3 => BuiltInDataTypeKind.Color3,
            TYPE_COLOR4 => BuiltInDataTypeKind.Color4,
            TYPE_SAMPLER => BuiltInDataTypeKind.Sampler,
            TYPE_TEXTURE1D => BuiltInDataTypeKind.Texture1D,
            TYPE_TEXTURE1DARRAY => BuiltInDataTypeKind.ArrayTexture1D,
            TYPE_TEXTURE2D => BuiltInDataTypeKind.Texture2D,
            TYPE_TEXTURE2DARRAY => BuiltInDataTypeKind.ArrayTexture2D,
            TYPE_TEXTURE3D => BuiltInDataTypeKind.Texture3D,
            TYPE_CUBEMAP => BuiltInDataTypeKind.Cubemap,
            TYPE_CUBEMAPARRAY => BuiltInDataTypeKind.ArrayCubemap,
            _ => throw new NotSupportedException("The given primitive type is not supported."),
        };

        var languageType = context.Parent as LanguageDataTypeContext;
        bool isLanguageType = languageType is not null;

        return new BuiltInDataType(kind)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            IsArray = isLanguageType && languageType!.IsArray,
            ArraySize = isLanguageType && languageType!.ArraySize != null
                ? uint.Parse(languageType!.ArraySize.Text.TrimEnd('u', 'U'))
                : null
        };
    }

    public override IDataType VisitUserDefinedDataType(UserDefinedDataTypeContext context)
    {
        var customType = context.Parent as CustomDataTypeContext;
        bool isCustomType = customType is not null;

        return new UserDefinedDataType(context.Type.ToNamespaceReference(_fileSource))
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            IsArray = isCustomType && customType!.IsArray,
            ArraySize = isCustomType && customType!.ArraySize != null
                ? uint.Parse(customType!.ArraySize.Text.TrimEnd('u', 'U'))
                : null
        };
    }
}
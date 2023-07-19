using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using Type = SPSL.Language.AST.Type;

namespace SPSL.Language.Visitors;

public class TypeVisitor : SPSLBaseVisitor<Type?>
{
    protected override Type? DefaultResult => null;

    protected override bool ShouldVisitNextChild(IRuleNode node, Type? currentResult)
        => node is SPSLParser.TypeContext or SPSLParser.StructContext or SPSLParser.EnumContext;

    public override Type VisitStruct(SPSLParser.StructContext context)
    {
        TypeKind tKind = TypeKind.Struct;
        var tName = context.Definition.Name.Text;
        Type type = new(tKind, tName)
        {
            ExtendedType = ASTVisitor.ParseNamespacedTypeName(context.Definition.ExtendedType),
        };

        // Register struct members
        foreach (SPSLParser.StructComponentContext member in context.structComponent())
        {
            if (member is SPSLParser.StructPropertyContext property)
            {
                var mType = property.Type.Accept(new DataTypeVisitor());
                var mName = property.Name.Text;
                type.AddProperty(new TypeProperty(mType, mName));
            }
            else if (member is SPSLParser.StructFunctionContext function)
            {
                var typeFunction = new TypeFunction(ASTVisitor.ParseFunction(function.Function));
                // TODO: Annotations
                type.AddFunction(typeFunction);
            }
        }

        return type;
    }

    public override Type VisitEnum(SPSLParser.EnumContext context)
    {
        TypeKind tKind = TypeKind.Enum;
        var tName = context.Definition.Name.Text;
        Type type = new(tKind, tName);

        uint lastValue = 0;

        // Register enum members
        foreach (SPSLParser.EnumComponentContext member in context.enumComponent())
        {
            var mName = member.Name.Text;
            IConstantExpression? value = (member.Value?.Accept(new ExpressionVisitor())) as IConstantExpression;

            if (value is not null && value is ILiteral constant)
            {
                lastValue = constant switch
                {
                    IntegerLiteral integer => (uint)integer.Value,
                    UnsignedIntegerLiteral integer => integer.Value,
                    _ => throw new NotSupportedException("The specified enum value is not supported. Only integer values are accepted.")
                };
            }

            type.AddProperty(new TypeProperty(new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger), mName) { Initializer = new UnsignedIntegerLiteral(lastValue) });
            lastValue++;
        }

        return type;
    }
}

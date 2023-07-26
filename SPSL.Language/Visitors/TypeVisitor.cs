using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using Type = SPSL.Language.AST.Type;

namespace SPSL.Language.Visitors;

public class TypeVisitor : SPSLBaseVisitor<Type?>
{
    private readonly string _fileSource;
    
    public TypeVisitor(string fileSource)
    {
        _fileSource = fileSource;
    }

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
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        // Register struct members
        foreach (SPSLParser.StructComponentContext member in context.structComponent())
        {
            switch (member)
            {
                case SPSLParser.StructPropertyContext property:
                {
                    var mType = property.Type.Accept(new DataTypeVisitor(_fileSource));
                    var mName = property.Name.Text;
                    type.AddProperty
                    (
                        new(mType, mName)
                        {
                            Start = property.Start.StartIndex,
                            End = property.Stop.StopIndex,
                            Source = _fileSource
                        }
                    );
                    break;
                }
                case SPSLParser.StructFunctionContext function:
                {
                    var typeFunction = new TypeFunction(function.Function.ToFunction(_fileSource))
                    {
                        Start = function.Start.StartIndex,
                        End = function.Stop.StopIndex,
                        Source = _fileSource
                    };
                    typeFunction.Annotations.AddRange(function.annotation().Select(a => a.ToAnnotation(_fileSource)));
                    type.AddFunction(typeFunction);
                    break;
                }
            }
        }

        return type;
    }

    public override Type VisitEnum(SPSLParser.EnumContext context)
    {
        TypeKind tKind = TypeKind.Enum;
        var tName = context.Definition.Name.Text;
        Type type = new(tKind, tName)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource
        };

        uint lastValue = 0;

        // Register enum members
        foreach (SPSLParser.EnumComponentContext member in context.enumComponent())
        {
            var mName = member.Name.Text;

            if (member.Value?.Accept(new ExpressionVisitor(_fileSource)) is ILiteral constant)
            {
                lastValue = constant switch
                {
                    IntegerLiteral integer => (uint)integer.Value,
                    UnsignedIntegerLiteral integer => integer.Value,
                    _ => throw new NotSupportedException("The specified enum value is not supported. Only integer values are accepted.")
                };
            }

            type.AddProperty
            (
                new(new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger), mName)
                {
                    Initializer = new UnsignedIntegerLiteral(lastValue),
                    Start = member.Start.StartIndex,
                    End = member.Stop.StopIndex,
                    Source = _fileSource
                }
            );
            lastValue++;
        }

        return type;
    }
}

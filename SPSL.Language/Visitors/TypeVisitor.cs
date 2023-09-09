using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Core;
using SPSL.Language.Utils;
using static SPSL.Language.SPSLParser;
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
        => node is TypeContext or StructContext or EnumContext;

    public override Type VisitStruct([NotNull] StructContext context)
    {
        const TypeKind tKind = TypeKind.Struct;
        var tName = context.Definition.Name.ToIdentifier(_fileSource);

        Type type = new(tKind, tName)
        {
            ExtendedType = context.Definition.ExtendedType.ToNamespaceReference(_fileSource),
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        // Register struct members
        foreach (StructComponentContext member in context.structComponent())
        {
            switch (member)
            {
                case StructPropertyContext property:
                {
                    var mType = property.Type.Accept(new DataTypeVisitor(_fileSource));
                    var mName = property.Name.ToIdentifier(_fileSource);

                    type.AddProperty
                    (
                        new(mType, mName)
                        {
                            Start = property.Start.StartIndex,
                            End = property.Stop.StopIndex,
                            Source = _fileSource,
                            Annotations = new(property.annotation().Select(a => a.ToAnnotation(_fileSource))),
                            Documentation = property.Documentation.ToDocumentation()
                        }
                    );
                    break;
                }
                case StructFunctionContext function:
                {
                    type.AddFunction
                    (
                        new(function.Function.ToFunction(_fileSource))
                        {
                            Start = function.Start.StartIndex,
                            End = function.Stop.StopIndex,
                            Source = _fileSource,
                            Annotations = new(function.annotation().Select(a => a.ToAnnotation(_fileSource))),
                        }
                    );
                    break;
                }
            }
        }

        return type;
    }

    public override Type VisitEnum([NotNull] EnumContext context)
    {
        const TypeKind tKind = TypeKind.Enum;
        var tName = context.Definition.Name.ToIdentifier(_fileSource);

        Type type = new(tKind, tName)
        {
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Source = _fileSource,
            Documentation = context.Documentation.ToDocumentation()
        };

        uint lastValue = 0;

        // Register enum members
        foreach (EnumComponentContext member in context.enumComponent())
        {
            var mName = member.Name.ToIdentifier(_fileSource);

            if (member.Value?.Accept(new ExpressionVisitor(_fileSource)) is ILiteral constant)
            {
                lastValue = constant switch
                {
                    IntegerLiteral integer => (uint)integer.Value,
                    UnsignedIntegerLiteral integer => integer.Value,
                    _ => throw new NotSupportedException(
                        "The specified enum value is not supported. Only integer values are accepted.")
                };
            }

            type.AddProperty
            (
                new(new PrimitiveDataType(PrimitiveDataTypeKind.UnsignedInteger), mName)
                {
                    Initializer = new UnsignedIntegerLiteral(lastValue),
                    Start = member.Start.StartIndex,
                    End = member.Stop.StopIndex,
                    Source = _fileSource,
                    Annotations = new(member.annotation().Select(a => a.ToAnnotation(_fileSource))),
                    Documentation = member.Documentation.ToDocumentation()
                }
            );

            lastValue++;
        }

        return type;
    }
}
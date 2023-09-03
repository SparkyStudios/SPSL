using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.AST;
using SPSL.Language.Symbols;
using SPSL.Language.Symbols.Modifiers;
using SPSL.Language.Utils;

namespace SPSL.Language.Visitors;

public class SymbolVisitor : SPSLBaseVisitor<Symbol>
{
    private readonly SymbolTable _globalSymbolTable;
    private readonly string _fileSource;

    private readonly Stack<SymbolTable> _stack = new();

    protected override Symbol DefaultResult => _globalSymbolTable;

    public SymbolVisitor(string fileSource)
    {
        _fileSource = fileSource;

        _globalSymbolTable = new()
        {
            Name = "__SPSL_GLOBAL_SYMBOL_TABLE__",
            Type = SymbolType.Scope,
            Source = fileSource,
        };
    }

    protected override Symbol AggregateResult(Symbol aggregate, Symbol nextResult)
    {
        return _globalSymbolTable;
    }

    public override Symbol VisitNamespaceDefinition([NotNull] SPSLParser.NamespaceDefinitionContext context)
    {
        _stack.Clear();

        var parts = context.Name.IDENTIFIER().Select(ns => ns.Symbol).ToArray();

        SymbolTable? ns = null;
        if (_globalSymbolTable.Contains(parts[0].Text))
        {
            ns = _globalSymbolTable.Lookup(parts[0].Text) as SymbolTable;
            for (uint i = 1; i < parts.Length; i++)
            {
                if (ns!.Contains(parts[i].Text))
                    ns = ns.Lookup(parts[i].Text) as SymbolTable;
                else
                    break;
            }
        }

        if (ns is null)
        {
            SymbolTable? parent = null;
            for (uint i = 0; i < parts.Length; i++)
            {
                ns = new()
                {
                    Name = parts[i].Text,
                    Type = SymbolType.Namespace,
                    Source = _fileSource,
                    Parent = parent
                };

                if (i == 0)
                    _globalSymbolTable.Add(ns);

                parent?.Add(ns);
                parent = ns;

                _stack.Push(parent);
            }
        }

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableBool([NotNull] SPSLParser.PermutationVariableBoolContext context)
    {
        SymbolTable permutation = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Permutation,
            Modifiers = new List<ISymbolModifier>
            {
                new PermutationTypeModifier(Core.PermutationVariableType.Bool)
            }
        };

        permutation.Add
        (
            new()
            {
                Name = context.Identifier.GetText(),
                Source = _fileSource,
                Start = context.Identifier.Start.StartIndex,
                End = context.Identifier.Stop.StopIndex,
                Type = SymbolType.Identifier,
            }
        );

        _stack.Peek().Add(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableEnum([NotNull] SPSLParser.PermutationVariableEnumContext context)
    {
        SymbolTable permutation = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Permutation,
            Modifiers = new List<ISymbolModifier>
            {
                new PermutationTypeModifier
                (
                    Core.PermutationVariableType.Enum,
                    context.IDENTIFIER().Select(value => value.Symbol.Text).ToArray()
                )
            }
        };

        permutation.Add
        (
            new()
            {
                Name = context.Identifier.GetText(),
                Source = _fileSource,
                Start = context.Identifier.Start.StartIndex,
                End = context.Identifier.Stop.StopIndex,
                Type = SymbolType.Identifier,
            }
        );

        foreach (ITerminalNode node in context.IDENTIFIER())
        {
            permutation.Add
            (
                new()
                {
                    Name = node.Symbol.Text,
                    Source = _fileSource,
                    Start = node.Symbol.StartIndex,
                    End = node.Symbol.StopIndex,
                    Type = SymbolType.Constant,
                }
            );
        }

        _stack.Peek().Add(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableInteger(
        [NotNull] SPSLParser.PermutationVariableIntegerContext context)
    {
        SymbolTable permutation = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Permutation,
            Modifiers = new List<ISymbolModifier>
            {
                new PermutationTypeModifier(Core.PermutationVariableType.Integer)
            }
        };

        permutation.Add
        (
            new()
            {
                Name = context.Identifier.GetText(),
                Source = _fileSource,
                Start = context.Identifier.Start.StartIndex,
                End = context.Identifier.Stop.StopIndex,
                Type = SymbolType.Identifier,
            }
        );

        _stack.Peek().Add(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitStruct([NotNull] SPSLParser.StructContext context)
    {
        SymbolTable structure = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Struct,
        };

        structure.Add
        (
            new()
            {
                Name = context.Definition.Name.Text,
                Source = _fileSource,
                Start = context.Definition.Name.StartIndex,
                End = context.Definition.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(structure);

        _stack.Push(structure);

        VisitChildren(context);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitStructProperty([NotNull] SPSLParser.StructPropertyContext context)
    {
        Symbol property = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Property,
        };

        _stack.Peek().Add(property);

        return _globalSymbolTable;
    }

    public override Symbol VisitStructFunction(SPSLParser.StructFunctionContext context)
    {
        SymbolTable function = new()
        {
            Name = context.Function.Head.Name.Text,
            Source = _fileSource,
            Start = context.Function.Start.StartIndex,
            End = context.Function.Stop.StopIndex,
            Type = SymbolType.Function,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Function.Head.Type.GetText()),
            }
        };

        function.Add
        (
            new()
            {
                Name = context.Function.Head.Name.Text,
                Source = _fileSource,
                Start = context.Function.Head.Name.StartIndex,
                End = context.Function.Head.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(function);

        _stack.Push(function);

        VisitChildren(context.Function);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitEnum([NotNull] SPSLParser.EnumContext context)
    {
        SymbolTable enumeration = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Enum,
        };

        enumeration.Add
        (
            new()
            {
                Name = context.Definition.Name.Text,
                Source = _fileSource,
                Start = context.Definition.Name.StartIndex,
                End = context.Definition.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(enumeration);

        _stack.Push(enumeration);

        VisitChildren(context);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitEnumComponent([NotNull] SPSLParser.EnumComponentContext context)
    {
        Symbol property = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Property,
        };

        _stack.Peek().Add(property);

        return _globalSymbolTable;
    }

    public override Symbol VisitInterface([NotNull] SPSLParser.InterfaceContext context)
    {
        SymbolTable @interface = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Interface,
        };

        foreach (SPSLParser.FunctionHeadContext function in context.functionHead())
        {
            Symbol f = new()
            {
                Name = function.Name.Text,
                Source = _fileSource,
                Start = function.Name.StartIndex,
                End = function.Name.StopIndex,
                Type = SymbolType.Function,
            };

            @interface.Add(f);
        }

        _stack.Peek().Add(@interface);

        return _globalSymbolTable;
    }

    public override Symbol VisitShaderFragment([NotNull] SPSLParser.ShaderFragmentContext context)
    {
        SymbolTable fragment = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Fragment,
        };

        fragment.Add
        (
            new()
            {
                Name = context.Definition.Name.Text,
                Source = _fileSource,
                Start = context.Definition.Name.StartIndex,
                End = context.Definition.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(fragment);

        _stack.Push(fragment);

        VisitChildren(context);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitShader([NotNull] SPSLParser.ShaderContext context)
    {
        SymbolTable shader = context.Definition switch
        {
            SPSLParser.GenericShaderDefinitionContext gsd => new()
            {
                Name = gsd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(gsd.Type.Text.ToShaderStage()),
                    new AbstractShaderModifier(gsd.IsAbstract)
                }
            },
            SPSLParser.ComputeShaderDefinitionContext csd => new()
            {
                Name = csd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(csd.Type.Text.ToShaderStage()),
                    new AbstractShaderModifier(csd.IsAbstract)
                }
            },
            _ => new()
            {
                Name = "__SPSL_UNKNOWN_SHADER__",
                Type = SymbolType.Invalid,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
            },
        };

        // TODO
        // shader.Add
        // (
        //     new()
        //     {
        //         Name = context.Definition.Name.Text,
        //         Source = _fileSource,
        //         Start = context.Definition.Name.StartIndex,
        //         End = context.Definition.Name.StopIndex,
        //         Type = SymbolType.Identifier
        //     }
        // );

        _stack.Peek().Add(shader);

        _stack.Push(shader);

        VisitChildren(context);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitBasicShaderFunction(SPSLParser.BasicShaderFunctionContext context)
    {
        SymbolTable function = new()
        {
            Name = $"{context.Function.Head.Name.Text}({string.Join(", ", context.Function.Head.Signature.Arguments?.argDef().Select(p => $"{p.Flow?.Text ?? "in"} {p.Type.GetText()}") ?? Array.Empty<string>())})",
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Function,
            Modifiers = new List<ISymbolModifier>
            {
                new ShaderFunctionOverrideModifier(context.IsOverride),
                new SymbolTypeModifier(context.Function.Head.Type.GetText()),
            }
        };

        function.Add
        (
            new()
            {
                Name = context.Function.Head.Name.Text,
                Source = _fileSource,
                Start = context.Function.Head.Name.StartIndex,
                End = context.Function.Head.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(function);

        _stack.Push(function);

        VisitChildren(context.Function);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitShaderConstructorFunction([NotNull] SPSLParser.ShaderConstructorFunctionContext context)
    {
        SymbolTable constructor = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Constructor
        };

        constructor.Add
        (
            new()
            {
                Name = context.Name.Text,
                Source = _fileSource,
                Start = context.Name.StartIndex,
                End = context.Name.StopIndex,
                Type = SymbolType.Identifier
            }
        );

        _stack.Peek().Add(constructor);

        _stack.Push(constructor);

        VisitChildren(context);

        _stack.Pop();

        return _globalSymbolTable;
    }

    public override Symbol VisitArgDef([NotNull] SPSLParser.ArgDefContext context)
    {
        Symbol argumentSymbol = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Parameter,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Type.GetText()),
                new ParameterFlowModifier(context.Flow?.Text.ToDataFlow() ?? Core.DataFlow.Unspecified),
            }
        };

        _stack.Peek().Add(argumentSymbol);

        return _globalSymbolTable;
    }

    public override Symbol VisitTypedVariableDeclaration(SPSLParser.TypedVariableDeclarationContext context)
    {
        ConstVariableModifier constModifier = new(context.IsConst);
        SymbolTypeModifier typeModifier = new(context.Type.GetText());

        foreach (var variableIdentityContext in context.variableIdentity())
        {
            if (variableIdentityContext.IsAssignment)
            {
                Symbol variable = new()
                {
                    Name = variableIdentityContext.Declaration.Identifier.Identifier.Text,
                    Source = _fileSource,
                    Start = variableIdentityContext.Declaration.Identifier.Identifier.StartIndex,
                    End = variableIdentityContext.Declaration.Identifier.Identifier.StopIndex,
                    Type = SymbolType.Variable,
                    Modifiers = new List<ISymbolModifier>
                    {
                        constModifier,
                        typeModifier,
                    }
                };

                _stack.Peek().Add(variable);
            }
            else
            {
                Symbol variable = new()
                {
                    Name = variableIdentityContext.Identifier.Identifier.Text,
                    Source = _fileSource,
                    Start = variableIdentityContext.Identifier.Identifier.StartIndex,
                    End = variableIdentityContext.Identifier.Identifier.StopIndex,
                    Type = SymbolType.Variable,
                    Modifiers = new List<ISymbolModifier>
                    {
                        constModifier,
                        typeModifier,
                    }
                };

                _stack.Peek().Add(variable);
            }
        }

        return _globalSymbolTable;
    }

    public override Symbol VisitUntypedVariableDeclaration(SPSLParser.UntypedVariableDeclarationContext context)
    {
        Symbol variable = new()
        {
            Name = context.Identifier.Identifier.Text,
            Source = _fileSource,
            Start = context.Identifier.Identifier.StartIndex,
            End = context.Identifier.Identifier.StopIndex,
            Type = SymbolType.Variable,
            Modifiers = new List<ISymbolModifier>
            {
                new ConstVariableModifier(context.IsConst),
                new SymbolTypeModifier
                (
                    context.Expression.Accept(new ExpressionVisitor(_fileSource)) switch
                    {
                        BoolLiteral => "bool",
                        DoubleLiteral => "double",
                        FloatLiteral => "float",
                        IntegerLiteral => "int",
                        UnsignedIntegerLiteral => "uint",
                        StringLiteral => "string",
                        _ => throw new NotImplementedException()
                    }
                ),
            }
        };

        _stack.Peek().Add(variable);

        return _globalSymbolTable;
    }
}
using System.Collections;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using SPSL.Language.Exceptions;
using SPSL.Language.Symbols;
using SPSL.Language.Symbols.Modifiers;
using SPSL.Language.Utils;
using static SPSL.Language.Exceptions.SemanticException;
using static SPSL.Language.SPSLParser;
using BoolLiteral = SPSL.Language.AST.BoolLiteral;
using DoubleLiteral = SPSL.Language.AST.DoubleLiteral;
using FloatLiteral = SPSL.Language.AST.FloatLiteral;
using IntegerLiteral = SPSL.Language.AST.IntegerLiteral;
using StringLiteral = SPSL.Language.AST.StringLiteral;
using UnsignedIntegerLiteral = SPSL.Language.AST.UnsignedIntegerLiteral;

namespace SPSL.Language.Visitors;

public class SymbolVisitor : SPSLBaseVisitor<Symbol>
{
    private readonly SymbolTable _globalSymbolTable;
    private readonly string _fileSource;

    private readonly Stack<Hashtable> _scopes = new();
    private readonly Stack<SymbolTable> _tables = new();

    private SymbolTable CurrentTable => _tables.Peek();
    private Hashtable CurrentScope => _scopes.Peek();

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

    private void BeginScope(SymbolTable scope)
    {
        _tables.Push(scope);
        _scopes.Push(new());
    }

    private void DeclareSymbol(Symbol symbol)
    {
        CheckSymbolNotExists(symbol);

        CurrentTable.Add(symbol);
        CurrentScope.Add(symbol.Name, false);
    }

    private void DefineSymbol(Symbol symbol)
    {
        CheckSymbolExists(symbol);

        CurrentScope[symbol.Name] = true;
    }

    private void CheckSymbolNotExists(Symbol symbol)
    {
        if (CurrentTable.Contains(symbol.Name))
            throw new SemanticException
            (
                $"Symbol '{symbol.Name}' already defined.",
                SemanticExceptionType.DuplicateSymbol,
                symbol
            );
    }

    private void CheckSymbolExists(Symbol symbol)
    {
        if (CurrentTable.Resolve(symbol.Name) == null)
            throw new SemanticException
            (
                $"Symbol '{symbol.Name}' is not defined",
                SemanticExceptionType.SymbolNotDeclared,
                symbol
            );
    }

    private void EndScope()
    {
        _tables.Pop();
        _scopes.Pop();
    }

    private void DefineSymbol(BasicExpressionContext context)
    {
        Symbol variable = new()
        {
            Name = context.Identifier.Text,
            Source = _fileSource,
            Start = context.Identifier.StartIndex,
            End = context.Identifier.StopIndex,
            Type = SymbolType.LocalVariable,
        };

        DefineSymbol(variable);
    }

    protected override Symbol AggregateResult(Symbol aggregate, Symbol nextResult)
    {
        return _globalSymbolTable;
    }

    public override Symbol VisitNamespaceDefinition([NotNull] NamespaceDefinitionContext context)
    {
        _scopes.Clear();

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

                BeginScope(parent);
            }
        }

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableBool([NotNull] PermutationVariableBoolContext context)
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

        BeginScope(permutation);

        Symbol symbol = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Identifier.Start.StartIndex,
            End = context.Identifier.Stop.StopIndex,
            Type = SymbolType.Identifier,
        };

        DeclareSymbol(symbol);
        DefineSymbol(symbol);

        EndScope();

        DeclareSymbol(permutation);
        DefineSymbol(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableEnum([NotNull] PermutationVariableEnumContext context)
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


        BeginScope(permutation);

        Symbol symbol = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Identifier.Start.StartIndex,
            End = context.Identifier.Stop.StopIndex,
            Type = SymbolType.Identifier,
        };

        DeclareSymbol(symbol);
        DefineSymbol(symbol);

        foreach (ITerminalNode node in context.IDENTIFIER())
        {
            Symbol enumValue = new()
            {
                Name = node.Symbol.Text,
                Source = _fileSource,
                Start = node.Symbol.StartIndex,
                End = node.Symbol.StopIndex,
                Type = SymbolType.Constant,
            };

            DeclareSymbol(enumValue);
            DefineSymbol(enumValue);
        }

        EndScope();

        DeclareSymbol(permutation);
        DefineSymbol(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitPermutationVariableInteger([NotNull] PermutationVariableIntegerContext context)
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

        BeginScope(permutation);

        Symbol symbol = new()
        {
            Name = context.Identifier.GetText(),
            Source = _fileSource,
            Start = context.Identifier.Start.StartIndex,
            End = context.Identifier.Stop.StopIndex,
            Type = SymbolType.Identifier,
        };

        DeclareSymbol(symbol);
        DefineSymbol(symbol);

        EndScope();

        DeclareSymbol(permutation);
        DefineSymbol(permutation);

        return _globalSymbolTable;
    }

    public override Symbol VisitGlobalVariable(GlobalVariableContext context)
    {
        Symbol variable = new()
        {
            Name = context.Definition.Identifier.Identifier.Text,
            Source = _fileSource,
            Start = context.Definition.Identifier.Identifier.StartIndex,
            End = context.Definition.Identifier.Identifier.StopIndex,
            Type = SymbolType.GlobalVariable,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Type.GetText()),
                // TODO: Add modifiers for static variables
            }
        };

        DeclareSymbol(variable);
        DefineSymbol(variable);

        return _globalSymbolTable;
    }

    public override Symbol VisitStruct([NotNull] StructContext context)
    {
        SymbolTable structure = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Struct,
        };

        BeginScope(structure);

        Symbol symbol = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Definition.Name.StartIndex,
            End = context.Definition.Name.StopIndex,
            Type = SymbolType.Identifier
        };

        DeclareSymbol(symbol);
        DefineSymbol(symbol);

        VisitChildren(context);

        EndScope();

        DeclareSymbol(structure);
        DefineSymbol(structure);

        return _globalSymbolTable;
    }

    public override Symbol VisitStructProperty([NotNull] StructPropertyContext context)
    {
        Symbol property = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Property,
        };

        DeclareSymbol(property);
        DefineSymbol(property);

        return _globalSymbolTable;
    }

    public override Symbol VisitStructFunction(StructFunctionContext context)
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

        BeginScope(function);

        Symbol symbol = new()
        {
            Name = context.Function.Head.Name.Text,
            Source = _fileSource,
            Start = context.Function.Head.Name.StartIndex,
            End = context.Function.Head.Name.StopIndex,
            Type = SymbolType.Identifier
        };

        DeclareSymbol(symbol);
        DefineSymbol(symbol);

        VisitChildren(context.Function);

        EndScope();

        DeclareSymbol(function);
        DefineSymbol(function);

        return _globalSymbolTable;
    }

    public override Symbol VisitEnum([NotNull] EnumContext context)
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

        CurrentTable.Add(enumeration);

        BeginScope(enumeration);

        VisitChildren(context);

        EndScope();

        return _globalSymbolTable;
    }

    public override Symbol VisitEnumComponent([NotNull] EnumComponentContext context)
    {
        Symbol property = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Property,
        };

        CurrentTable.Add(property);

        return _globalSymbolTable;
    }

    public override Symbol VisitInterface([NotNull] InterfaceContext context)
    {
        SymbolTable @interface = new()
        {
            Name = context.Definition.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Interface,
        };

        foreach (FunctionHeadContext function in context.functionHead())
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

        CurrentTable.Add(@interface);

        return _globalSymbolTable;
    }

    public override Symbol VisitShaderFragment([NotNull] ShaderFragmentContext context)
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

        CurrentTable.Add(fragment);

        BeginScope(fragment);

        VisitChildren(context);

        EndScope();

        return _globalSymbolTable;
    }

    public override Symbol VisitShader([NotNull] ShaderContext context)
    {
        SymbolTable shader = context.Definition switch
        {
            GenericShaderDefinitionContext gsd => new()
            {
                Name = gsd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(gsd.Type.ToShaderStage()),
                    new AbstractShaderModifier(gsd.IsAbstract)
                }
            },
            ComputeShaderDefinitionContext csd => new()
            {
                Name = csd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(csd.Type.ToShaderStage()),
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

        CurrentTable.Add(shader);

        BeginScope(shader);

        VisitChildren(context);

        EndScope();

        return _globalSymbolTable;
    }

    public override Symbol VisitInPlaceStructuredBufferDefinition(InPlaceStructuredBufferDefinitionContext context)
    {
        SymbolTable buffer = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex,
            Type = SymbolType.Buffer,
        };

        buffer.Add
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

        DeclareSymbol(buffer);

        BeginScope(buffer);

        VisitChildren(context);

        EndScope();

        DefineSymbol(buffer);

        return _globalSymbolTable;
    }

    public override Symbol VisitTypedBufferDefinition(TypedBufferDefinitionContext context)
    {
        Symbol buffer = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Buffer,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Type.GetText())
            }
        };

        DeclareSymbol(buffer);
        DefineSymbol(buffer);

        return _globalSymbolTable;
    }

    public override Symbol VisitBufferComponent(BufferComponentContext context)
    {
        Symbol component = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Property,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Type.GetText())
            }
        };

        if (context.Parent is not StreamPropertyContext)
            DeclareSymbol(component);

        return _globalSymbolTable;
    }

    public override Symbol VisitBasicShaderFunction(BasicShaderFunctionContext context)
    {
        SymbolTable function = new()
        {
            Name =
                $"{context.Function.Head.Name.Text}({string.Join(", ", context.Function.Head.Signature.Arguments?.argDef().Select(p => $"{p.Flow?.Text ?? "in"} {p.Type.GetText()}") ?? Array.Empty<string>())})",
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

        CurrentTable.Add(function);

        BeginScope(function);

        VisitChildren(context.Function);

        EndScope();

        return _globalSymbolTable;
    }

    public override Symbol VisitShaderConstructorFunction([NotNull] ShaderConstructorFunctionContext context)
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

        CurrentTable.Add(constructor);

        BeginScope(constructor);

        Symbol inStreams = new()
        {
            Name = "in_streams",
            Source = _fileSource,
            Type = SymbolType.LocalVariable
        };
        DeclareSymbol(inStreams);
        DefineSymbol(inStreams);

        Symbol outStreams = new()
        {
            Name = "out_streams",
            Source = _fileSource,
            Type = SymbolType.LocalVariable
        };
        DeclareSymbol(outStreams);
        DefineSymbol(outStreams);

        VisitChildren(context);

        EndScope();

        return _globalSymbolTable;
    }

    public override Symbol VisitArgDef([NotNull] ArgDefContext context)
    {
        Symbol argument = new()
        {
            Name = context.Name.Text,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Type = SymbolType.Parameter,
            Modifiers = new List<ISymbolModifier>
            {
                new SymbolTypeModifier(context.Type.GetText()),
                new ParameterFlowModifier(context.Flow.ToDataFlow()),
            }
        };

        DeclareSymbol(argument);
        DefineSymbol(argument);

        return _globalSymbolTable;
    }

    public override Symbol VisitTypedVariableDeclaration(TypedVariableDeclarationContext context)
    {
        ConstVariableModifier constModifier = new(context.IsConst);
        SymbolTypeModifier typeModifier = new(context.Type.GetText());

        foreach (VariableIdentityContext variableIdentityContext in context.variableIdentity())
        {
            if (variableIdentityContext.IsAssignment)
            {
                Symbol variable = new()
                {
                    Name = variableIdentityContext.Declaration.Identifier.Identifier.Text,
                    Source = _fileSource,
                    Start = variableIdentityContext.Declaration.Identifier.Identifier.StartIndex,
                    End = variableIdentityContext.Declaration.Identifier.Identifier.StopIndex,
                    Type = SymbolType.LocalVariable,
                    Modifiers = new List<ISymbolModifier>
                    {
                        constModifier,
                        typeModifier,
                    }
                };

                DeclareSymbol(variable);
                DefineSymbol(variable);
            }
            else
            {
                Symbol variable = new()
                {
                    Name = variableIdentityContext.Identifier.Identifier.Text,
                    Source = _fileSource,
                    Start = variableIdentityContext.Identifier.Identifier.StartIndex,
                    End = variableIdentityContext.Identifier.Identifier.StopIndex,
                    Type = SymbolType.LocalVariable,
                    Modifiers = new List<ISymbolModifier>
                    {
                        constModifier,
                        typeModifier,
                    }
                };

                DeclareSymbol(variable);
            }
        }

        return _globalSymbolTable;
    }

    public override Symbol VisitUntypedVariableDeclaration(UntypedVariableDeclarationContext context)
    {
        Symbol variable = new()
        {
            Name = context.Identifier.Identifier.Text,
            Source = _fileSource,
            Start = context.Identifier.Identifier.StartIndex,
            End = context.Identifier.Identifier.StopIndex,
            Type = SymbolType.LocalVariable,
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

        DeclareSymbol(variable);
        DefineSymbol(variable);

        return _globalSymbolTable;
    }

    public override Symbol VisitAssignmentExpression(AssignmentExpressionContext context)
    {
        context.Left.Accept(this);
        context.Right.Accept(this);

        return _globalSymbolTable;
    }

    public override Symbol VisitAssignableExpression(AssignableExpressionContext context)
    {
        if (context.basicExpression() != null)
            DefineSymbol(context.basicExpression());
        // else if (context.arrayAccessExpression() != null)
        //     CheckSymbolExists(context.arrayAccessExpression());
        // else if (context.propertyMemberReferenceExpression() != null)
        //     CheckSymbolExists(context.propertyMemberReferenceExpression());
        // else if (context.assignableChainedExpression() != null)
        //     CheckSymbolExists(context.assignableChainedExpression());

        return _globalSymbolTable;
    }

    private void CheckSymbolExists(AssignableChainedExpressionContext context)
    {
        CheckSymbolExists(context.Target);
        foreach (AssignableChainableExpressionContext expressionContext in context.assignableChainableExpression())
            CheckSymbolExists(expressionContext);
    }

    private void CheckSymbolExists(AssignableChainableExpressionContext context)
    {
        if (context.basicExpression() != null)
            CheckSymbolExists(context.basicExpression());
        else if (context.arrayAccessExpression() != null)
            CheckSymbolExists(context.arrayAccessExpression());
    }

    private void CheckSymbolExists(ArrayAccessExpressionContext context)
    {
        if (context.basicExpression() != null)
            CheckSymbolExists(context.basicExpression());
        else if (context.memberReferenceExpression() != null)
            CheckSymbolExists(context.memberReferenceExpression());
    }

    private void CheckSymbolExists(MemberReferenceExpressionContext context)
    {
        if (context.propertyMemberReferenceExpression() != null)
            CheckSymbolExists(context.propertyMemberReferenceExpression());
        else if (context.methodMemberReferenceExpression() != null)
            CheckSymbolExists(context.methodMemberReferenceExpression());
    }

    private void CheckSymbolExists(MethodMemberReferenceExpressionContext context)
    {
        if (CurrentTable.Parent is not { Type: SymbolType.Shader or SymbolType.Fragment })
        {
            Symbol variable = new()
            {
                Name = context.Target.Identifier.Text,
                Source = _fileSource,
                Start = context.Target.Identifier.StartIndex,
                End = context.Target.Identifier.StopIndex,
                Type = SymbolType.Keyword,
            };

            throw new SemanticException
            (
                "Cannot use 'this' or 'base' outside of a shader or shader fragment.",
                SemanticExceptionType.ContextKeywordUsedOutOfContext,
                variable
            );
        }

        CheckSymbolExists(context.Member);
    }

    private void CheckSymbolExists(InvocationExpressionContext context)
    {
        Symbol variable = new()
        {
            Name = context.Name.GetText(),
            Source = _fileSource,
            Start = context.Name.Start.StartIndex,
            End = context.Name.Stop.StopIndex,
            Type = SymbolType.Identifier,
        };

        CheckSymbolExists(variable);

        if (context.Parameters == null) return;

        foreach (ExpressionStatementContext expression in context.Parameters.expressionStatement())
            CheckSymbolExists(expression);
    }

    private void CheckSymbolExists(ExpressionStatementContext context)
    {
        switch (context)
        {
            case ExpressionContext expressionContext:
                CheckSymbolExists(expressionContext);
                break;
        }
    }

    private void CheckSymbolExists(ExpressionContext context)
    {
        if (context.basicExpression() != null)
            CheckSymbolExists(context.basicExpression());
        else if (context.parenthesizedExpression() != null)
            CheckSymbolExists(context.parenthesizedExpression());
        else if (context.propertyMemberReferenceExpression() != null)
            CheckSymbolExists(context.propertyMemberReferenceExpression());
        else if (context.methodMemberReferenceExpression() != null)
            CheckSymbolExists(context.methodMemberReferenceExpression());
        else if (context.chainedExpression() != null)
            CheckSymbolExists(context.chainedExpression());
        else if (context.invocationExpression() != null)
            CheckSymbolExists(context.invocationExpression());
        else if (context.arrayAccessExpression() != null)
            CheckSymbolExists(context.arrayAccessExpression());
    }

    private void CheckSymbolExists(ChainedExpressionContext context)
    {
        CheckSymbolExists(context.Target);
        // TODO: Use types to check for this
        // foreach (ChainableExpressionContext expressionContext in context.chainableExpression())
        //     CheckSymbolExists(expressionContext);
    }

    private void CheckSymbolExists(ChainableExpressionContext context)
    {
        if (context.basicExpression() != null)
            CheckSymbolExists(context.basicExpression());
        else if (context.invocationExpression() != null)
            CheckSymbolExists(context.invocationExpression());
        else if (context.arrayAccessExpression() != null)
            CheckSymbolExists(context.arrayAccessExpression());
    }

    private void CheckSymbolExists(ReferencableExpressionContext context)
    {
        if (context.basicExpression() != null)
            CheckSymbolExists(context.basicExpression());
        else if (context.parenthesizedExpression() != null)
            CheckSymbolExists(context.parenthesizedExpression());
        else if (context.invocationExpression() != null)
            CheckSymbolExists(context.invocationExpression());
        else if (context.propertyMemberReferenceExpression() != null)
            CheckSymbolExists(context.propertyMemberReferenceExpression());
        else if (context.methodMemberReferenceExpression() != null)
            CheckSymbolExists(context.methodMemberReferenceExpression());
        else if (context.arrayAccessExpression() != null)
            CheckSymbolExists(context.arrayAccessExpression());
    }

    private void CheckSymbolExists(ParenthesizedExpressionContext context)
    {
        CheckSymbolExists(context.Expression);
    }

    private void CheckSymbolExists(PropertyMemberReferenceExpressionContext context)
    {
        if (CurrentTable.Parent is not { Type: SymbolType.Shader or SymbolType.Fragment })
        {
            Symbol variable = new()
            {
                Name = context.Target.Identifier.Text,
                Source = _fileSource,
                Start = context.Target.Identifier.StartIndex,
                End = context.Target.Identifier.StopIndex,
                Type = SymbolType.Keyword,
            };

            throw new SemanticException
            (
                "Cannot use 'this' or 'base' outside of a shader or shader fragment.",
                SemanticExceptionType.ContextKeywordUsedOutOfContext,
                variable
            );
        }

        CheckSymbolExists(context.Member);
    }

    private void CheckSymbolExists(BasicExpressionContext context)
    {
        Symbol variable = new()
        {
            Name = context.Identifier.Text,
            Source = _fileSource,
            Start = context.Identifier.StartIndex,
            End = context.Identifier.StopIndex,
            Type = SymbolType.LocalVariable,
        };

        CheckSymbolExists(variable);
    }
}
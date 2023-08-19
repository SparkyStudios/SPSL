using System.Collections.Immutable;
using SPSL.Language.Core;
using SPSL.Language.Symbols;
using SPSL.Language.Utils;
using SPSL.Language.Symbols.Modifiers;

namespace SPSL.Language.Listeners;

public class SymbolListener : SPSLBaseListener
{
    private readonly Stack<SymbolTable> _stack = new();
    private readonly string _fileSource;

    public SymbolListener(string fileSource)
    {
        _fileSource = fileSource;

        GlobalSymbolTable = new()
        {
            Type = SymbolType.Scope,
            Name = "__SPSL__GLOBAL__SCOPE__",
            Source = _fileSource
        };
    }

    public SymbolTable GlobalSymbolTable { get; }

    public override void EnterNamespaceDefinition(SPSLParser.NamespaceDefinitionContext context)
    {
        SymbolTable ns = new()
        {
            Name = context.Name.GetText(),
            Type = SymbolType.Namespace,
            Source = _fileSource,
            Start = context.Name.Start.StartIndex,
            End = context.Name.Stop.StopIndex
        };

        GlobalSymbolTable.Add(ns);

        _stack.Push(ns);
    }

    public override void EnterShader(SPSLParser.ShaderContext context)
    {
        SymbolTable shader = context.Definition switch
        {
            SPSLParser.GenericShaderDefinitionContext gsd => new()
            {
                Name = gsd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = gsd.Name.StartIndex,
                End = gsd.Name.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(gsd.Type.Text.ToShaderStage())
                }
            },
            SPSLParser.ComputeShaderDefinitionContext csd => new()
            {
                Name = csd.Name.Text,
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = csd.Name.StartIndex,
                End = csd.Name.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderStageModifier(csd.Type.Text.ToShaderStage())
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

        _stack.Peek().Add(shader);

        _stack.Push(shader);
    }

    public override void ExitShader(SPSLParser.ShaderContext context)
    {
        _stack.Pop();
    }

    public override void EnterGlobalVariable(SPSLParser.GlobalVariableContext context)
    {
        Symbol variable = new()
        {
            Name = context.Definition.Identifier.GetText(),
            Type = SymbolType.Variable,
            Source = _fileSource,
            Start = context.Definition.Identifier.Start.StartIndex,
            End = context.Definition.Identifier.Stop.StopIndex
        };

        _stack.Peek().Add(variable);
    }

    public override void EnterInPlaceStructuredBufferDefinition
    (
        SPSLParser.InPlaceStructuredBufferDefinitionContext context
    )
    {
        SymbolTable buffer = new()
        {
            Name = context.Name.Text,
            Type = SymbolType.Buffer,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Modifiers = new List<ISymbolModifier>
            {
                new BufferStorageModifier
                (
                    context.Storage == null
                        ? BufferStorage.Undefined
                        : context.Storage.Text.ToBufferStorage()
                ),
                new BufferAccessModifier(context.Access.Text.ToBufferAccess())
            }
        };

        _stack.Peek().Add(buffer);

        _stack.Push(buffer);
    }

    public override void ExitInPlaceStructuredBufferDefinition
    (
        SPSLParser.InPlaceStructuredBufferDefinitionContext context
    )
    {
        _stack.Pop();
    }

    public override void EnterTypedBufferDefinition(SPSLParser.TypedBufferDefinitionContext context)
    {
        Symbol buffer = new()
        {
            Name = context.Name.Text,
            Type = SymbolType.Buffer,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex,
            Modifiers = new List<ISymbolModifier>
            {
                new BufferStorageModifier
                (
                    context.Storage == null
                        ? BufferStorage.Undefined
                        : context.Storage.Text.ToBufferStorage()
                ),
                new BufferAccessModifier(context.Access.Text.ToBufferAccess())
            }
        };

        _stack.Peek().Add(buffer);
    }

    public override void EnterStruct(SPSLParser.StructContext context)
    {
        SymbolTable structType = new()
        {
            Name = context.Definition.Name.Text,
            Type = SymbolType.Struct,
            Source = _fileSource,
            Start = context.Definition.Start.StartIndex,
            End = context.Definition.Stop.StopIndex
        };

        _stack.Peek().Add(structType);

        _stack.Push(structType);
    }

    public override void ExitStruct(SPSLParser.StructContext context)
    {
        _stack.Pop();
    }

    public override void EnterStructProperty(SPSLParser.StructPropertyContext context)
    {
        Symbol property = new()
        {
            Name = context.Name.Text,
            Type = SymbolType.Property,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex
        };

        _stack.Peek().Add(property);
    }

    public override void EnterFunction(SPSLParser.FunctionContext context)
    {
        SymbolTable function = new()
        {
            Name = context.Head.Name.Text,
            Type = SymbolType.Function,
            Source = _fileSource,
            Start = context.Head.Name.StartIndex,
            End = context.Head.Name.StopIndex
        };

        _stack.Peek().Add(function);

        _stack.Push(function);
    }

    public override void ExitFunction(SPSLParser.FunctionContext context)
    {
        _stack.Pop();
    }

    public override void EnterArgDef(SPSLParser.ArgDefContext context)
    {
        Symbol parameter = new()
        {
            Name = context.Name.Text,
            Type = SymbolType.Parameter,
            Source = _fileSource,
            Start = context.Name.StartIndex,
            End = context.Name.StopIndex
        };

        _stack.Peek().Add(parameter);
    }

    public override void ExitTypedVariableDeclaration(SPSLParser.TypedVariableDeclarationContext context)
    {
        //Symbol variable = _stack.Peek();

        //if (variable.Type != SymbolType.Variable)
        //    throw new("Unexpected symbol type");
    }

    public override void EnterVariableIdentity(SPSLParser.VariableIdentityContext context)
    {
        if (context.IsAssignment)
            return;

        Symbol variable = new()
        {
            Name = context.Identifier.Identifier.Text,
            Type = SymbolType.Variable,
            Source = _fileSource,
            Start = context.Identifier.Identifier.StartIndex,
            End = context.Identifier.Identifier.StopIndex
        };

        _stack.Peek().Add(variable);
    }

    public override void EnterVariableDeclarationAssignment(SPSLParser.VariableDeclarationAssignmentContext context)
    {
        Symbol variable = new()
        {
            Name = context.Identifier.Identifier.Text,
            Type = SymbolType.Variable,
            Source = _fileSource,
            Start = context.Identifier.Identifier.StartIndex,
            End = context.Identifier.Identifier.StopIndex
        };

        _stack.Peek().Add(variable);
    }
}
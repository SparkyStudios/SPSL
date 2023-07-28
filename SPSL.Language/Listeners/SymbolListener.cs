using SPSL.Language.Symbols;

namespace SPSL.Language.Listeners;

public class SymbolListener : SPSLBaseListener
{
    private readonly Stack<SymbolTable> _stack = new();
    private readonly string _fileSource;

    public SymbolListener(string fileSource)
    {
        _fileSource = fileSource;
    }

    public SymbolTable GlobalSymbolTable { get; } = new();

    public override void EnterNamespaceDefinition(SPSLParser.NamespaceDefinitionContext context)
    {
        SymbolTable ns = new()
        {
            Name = context.Name.GetText(),
            Type = SymbolType.Namespace,
            Source = _fileSource,
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
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
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderTypeModifier(gsd.Type.Text)
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
                    new ShaderTypeModifier(csd.Type.Text)
                }
            },
            _ => new()
            {
                Name = "__SPSL_UNKNOWN_SHADER__",
                Type = SymbolType.Shader,
                Source = _fileSource,
                Start = context.Start.StartIndex,
                End = context.Stop.StopIndex,
                Modifiers = new List<ISymbolModifier>
                {
                    new ShaderTypeModifier("__SPSL_UNKNOWN_SHADER_MODIFIER__")
                }
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
            Start = context.Start.StartIndex,
            End = context.Stop.StopIndex
        };
        
        _stack.Peek().Add(variable);
    }
}
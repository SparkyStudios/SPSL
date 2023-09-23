using SPSL.Language.Analysis.Common;
using SPSL.Language.Analysis.Symbols;
using SPSL.Language.Parsing.AST;
using SPSL.Language.Parsing.Common;

namespace SPSL.CommandLine.Utils;

/// <summary>
/// Generates the possible variants by combining all the available values per permutations.
/// Inspired from https://github.com/ezEngine/ezEngine/blob/dev/Code/Engine/RendererCore/ShaderCompiler/Implementation/PermutationGenerator.cpp
/// </summary>
public class VariantGenerator
{
    private readonly Dictionary<string, HashSet<string>> _permutations = new();
    private readonly Ast _ast;
    private readonly SymbolTable _symbolTable;

    public VariantGenerator(Ast ast, SymbolTable symbolTable)
    {
        _ast = ast;
        _symbolTable = symbolTable;
    }

    public void Clear()
    {
        _permutations.Clear();
    }

    public bool Add(string name)
    {
        if (!_permutations.ContainsKey(name))
            _permutations.Add(name, new());

        Symbol? symbol = _symbolTable.LookupInCurrentAndChildTables(name, SymbolType.Permutation);
        if (symbol == null)
            return false;

        if (_ast.ResolveNode(symbol.Source, symbol.Start) is not PermutationVariable node)
            return false;

        switch (node.Type)
        {
            case PermutationVariableType.Bool:
            {
                _permutations[name].Add("true");
                _permutations[name].Add("false");
                break;
            }
            case PermutationVariableType.Enum:
            {
                foreach (Identifier value in node.EnumerationValues)
                    _permutations[name].Add(value.Value);
                break;
            }
            case PermutationVariableType.Integer:
            {
                Annotation? annotation =
                    node.Annotations.LastOrDefault(annotation => annotation.Identifier.Value is "range" or "values");
                if (annotation == null)
                    return false;

                switch (annotation.Identifier.Value)
                {
                    case "range":
                    {
                        int min = ((IntegerLiteral)annotation.Arguments[0]).Value;
                        int max = ((IntegerLiteral)annotation.Arguments[1]).Value;
                        for (int i = min; i < max; i++)
                            _permutations[name].Add(i.ToString());
                        break;
                    }
                    case "values":
                    {
                        foreach (IExpression argument in annotation.Arguments)
                        {
                            int value = ((IntegerLiteral)argument).Value;
                            _permutations[name].Add(value.ToString());
                        }

                        break;
                    }
                }

                break;
            }
        }

        return true;
    }

    public bool Remove(string name)
    {
        return _permutations.Remove(name);
    }

    public uint GetVariantCount()
    {
        return _permutations.Values.Aggregate((uint)1, (current, value) => current * (uint)value.Count);
    }

    public List<PermutationValue> GetVariant(uint index)
    {
        List<PermutationValue> result = new();

        foreach (var permutation in _permutations)
        {
            var valuesCount = (uint)permutation.Value.Count;
            uint useValue = index % valuesCount;

            index /= valuesCount;

            foreach (string value in permutation.Value)
            {
                if (useValue != 0)
                {
                    --useValue;
                    continue;
                }

                PermutationValue permutationValue = new($"{permutation.Key}={value}");
                result.Add(permutationValue);
                break;
            }
        }

        return result;
    }
}
using SPSL.Language.Utils;

namespace SPSL.Language.Parsing.AST;

public interface IBlock : INode
{
    OrderedSet<IBlockChild> Children { get; }
}
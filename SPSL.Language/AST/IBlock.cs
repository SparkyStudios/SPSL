using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public interface IBlock : INode
{
    OrderedSet<IBlockChild> Children { get; }
}
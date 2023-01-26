using SPSL.Language.Utils;

namespace SPSL.Language.AST;

public interface IBlock
{
    OrderedSet<IBlockChild> Children { get; }
}
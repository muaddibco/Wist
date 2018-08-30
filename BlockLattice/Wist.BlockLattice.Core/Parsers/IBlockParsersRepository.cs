using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParsersRepository : IRepository<IBlockParser, ushort>
    {
        PacketType PacketType { get; }
    }
}

using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Parsers
{
    [ServiceContract]
    public interface IBlockParsersRepositoriesRepository
    {
        IBlockParsersRepository GetBlockParsersRepository(PacketType packetType);
    }
}

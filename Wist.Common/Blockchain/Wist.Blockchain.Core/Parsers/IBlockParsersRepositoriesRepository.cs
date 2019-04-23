using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Blockchain.Core.Parsers
{
    [ServiceContract]
    public interface IBlockParsersRepositoriesRepository
    {
        IBlockParsersRepository GetBlockParsersRepository(PacketType packetType);
    }
}

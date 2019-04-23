using Wist.Blockchain.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Blockchain.Core.Parsers
{
    [ExtensionPoint]
    public interface IBlockParsersRepository : IRepository<IBlockParser, ushort>
    {
        PacketType PacketType { get; }
    }
}

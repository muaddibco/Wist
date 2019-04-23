using System.Threading;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Interfaces
{
    [ExtensionPoint]
    public interface IBlocksHandler
    {
        string Name { get; }

        PacketType PacketType { get; }

        void Initialize(CancellationToken ct);
        void ProcessBlock(PacketBase blockBase);
    }
}

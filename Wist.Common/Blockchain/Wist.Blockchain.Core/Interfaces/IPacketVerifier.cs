using Wist.Core.Architecture;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Interfaces
{
    [ExtensionPoint]
    public interface IPacketVerifier
    {
        PacketType PacketType { get; }

        bool ValidatePacket(PacketBase block);
    }
}

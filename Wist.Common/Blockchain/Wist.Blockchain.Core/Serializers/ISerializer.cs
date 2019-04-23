using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers
{
    [ExtensionPoint]
    public interface ISerializer : IPacketProvider, ITransactionKeyProvider
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        void Initialize(PacketBase blockBase);

        void SerializeBody();

        void SerializeFully();
    }
}

using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers.RawPackets
{
    [ExtensionPoint]
    public interface IRawPacketProvider : IPacketProvider, ITransactionKeyProvider
    {
        void Initialize(IPacket blockBase);
    }
}

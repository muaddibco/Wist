using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Serializers.RawPackets
{
    [ServiceContract]
    public interface IRawPacketProvidersFactory : IFactory<IRawPacketProvider>
    {
        IRawPacketProvider Create(IPacket blockBase);
    }
}

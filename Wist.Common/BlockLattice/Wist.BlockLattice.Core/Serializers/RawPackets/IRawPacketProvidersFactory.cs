using Wist.BlockLattice.Core.DataModel;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Serializers.RawPackets
{
    [ServiceContract]
    public interface IRawPacketProvidersFactory : IFactory<IRawPacketProvider>
    {
        IRawPacketProvider Create(IBlockBase blockBase);
    }
}

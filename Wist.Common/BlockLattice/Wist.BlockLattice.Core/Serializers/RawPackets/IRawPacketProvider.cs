using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.BlockLattice.Core.Serializers.RawPackets
{
    [ExtensionPoint]
    public interface IRawPacketProvider : IPacketProvider, ITransactionKeyProvider
    {
        void Initialize(IBlockBase blockBase);
    }
}

using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Serializers.RawPackets
{

    [RegisterExtension(typeof(IRawPacketProvider), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RawPacketProvider : IRawPacketProvider
    {
        private bool _disposed = false; // To detect redundant calls
        private BlockBase _blockBase;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;

        public RawPacketProvider(IRawPacketProvidersFactory rawPacketProvidersFactory)
        {
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
        }

        public byte[] GetBytes()
        {
            return _blockBase?.RawData;
        }

        public void Initialize(BlockBase blockBase)
        {
            _disposed = false;
            _blockBase = blockBase;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _rawPacketProvidersFactory.Utilize(this);

                _disposed = true;
            }
        }
    }
}

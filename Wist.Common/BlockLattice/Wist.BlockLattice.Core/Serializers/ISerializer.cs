using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.BlockLattice.Core.Serializers
{
    [ExtensionPoint]
    public interface ISerializer : IPacketProvider, ITransactionKeyProvider
    {
        PacketType PacketType { get; }

        ushort BlockType { get; }

        void Initialize(BlockBase blockBase);

        void FillBodyAndRowBytes();
    }
}

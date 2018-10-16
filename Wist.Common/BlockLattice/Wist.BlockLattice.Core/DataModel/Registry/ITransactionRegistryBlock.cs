using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public interface ITransactionRegistryBlock : IBlockBase
    {
        ulong SyncBlockHeight { get; }

        ITransactionSourceKey TransactionSourceKey { get; }

        PacketType ReferencedPacketType { get; }

        ushort ReferencedBlockType { get; }

        byte[] ReferencedBodyHash { get; }

        byte[] ReferencedTarget { get; }
    }
}

using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Storage
{
    public abstract class StorageBlockBase : SyncedBlockBase
    {
        public override PacketType PacketType => PacketType.Storage;
    }
}

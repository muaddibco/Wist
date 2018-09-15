using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public abstract class RegistryBlockBase : SyncedBlockBase
    {
        public override PacketType PacketType => PacketType.Registry;
    }
}

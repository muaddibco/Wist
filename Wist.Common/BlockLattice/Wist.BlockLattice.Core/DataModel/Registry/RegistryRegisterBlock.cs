using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryRegisterBlock : RegistryBlockBase, ITransactionRegistryBlock
    {
        public override ushort BlockType => BlockTypes.Registry_Register;

        public override ushort Version => 1;

        public ITransactionSourceKey TransactionSourceKey => new AccountSourceKey(this);

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTarget { get; set; }
    }
}

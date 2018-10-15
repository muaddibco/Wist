using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryRegisterUtxoConfidentialBlock : UtxoConfidentialBase, ITransactionRegistryBlock<UtxoConfidentialSourceKey>
    {
        public override PacketType PacketType => PacketType.Registry;

        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Registry_RegisterUtxoConfidential;

        public ITransactionSourceKey<UtxoConfidentialSourceKey> TransactionSourceKey => new UtxoConfidentialSourceKey(this);

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTarget { get; set; }
    }
}

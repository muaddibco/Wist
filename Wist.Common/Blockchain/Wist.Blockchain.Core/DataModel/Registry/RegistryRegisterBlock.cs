using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Registry
{
    public class RegistryRegisterBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_Register;

        public override ushort Version => 1;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTarget { get; set; }

		public byte[] ReferencedTransactionKey { get; set; }
	}
}

using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Registry
{
    public class RegistryRegisterUtxoConfidential : UtxoConfidentialBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Registry;

        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.Registry_RegisterUtxoConfidential;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

		public byte[] ReferencedBodyHash { get; set; }
    }
}

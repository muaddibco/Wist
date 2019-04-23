using System.IO;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Serializers.UtxoConfidential;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryRegisterUtxoConfidentialBlockSerializer : UtxoConfidentialSerializerBase<RegistryRegisterUtxoConfidential>
    {
        public RegistryRegisterUtxoConfidentialBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_RegisterUtxoConfidential)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedPacketType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedBodyHash);
        }
    }
}

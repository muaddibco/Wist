using System.IO;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryConfidenceBlockSerializer : SignatureSupportSerializerBase<RegistryConfidenceBlock>
    {
        public RegistryConfidenceBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_ConfidenceBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.BitMask.Length);
            bw.Write(_block.BitMask);
            bw.Write(_block.ConfidenceProof);
            bw.Write(_block.ReferencedBlockHash);
        }
    }
}

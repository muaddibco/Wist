using System.IO;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryRegisterBlockSerializer : SignatureSupportSerializerBase<RegistryRegisterBlock>
    {
        public RegistryRegisterBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_Register)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedPacketType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedBodyHash);
            bw.Write(_block.ReferencedTarget);

			if((_block.ReferencedBlockType & BlockTypes.TransitionalFlag) == BlockTypes.TransitionalFlag)
			{
				bw.Write(_block.ReferencedTransactionKey);
			}
		}
    }
}

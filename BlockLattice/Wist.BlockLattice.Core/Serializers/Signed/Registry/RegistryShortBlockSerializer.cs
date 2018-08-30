using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryShortBlockSerializer : SyncSupportSerializerBase<RegistryShortBlock>
    {
        public RegistryShortBlockSerializer(PacketType packetType, ushort blockType, ICryptoService cryptoService) : base(packetType, blockType, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.TransactionHeaderHashes.Count);

            foreach (var item in _block.TransactionHeaderHashes)
            {
                bw.Write(item.Key);
                bw.Write(item.Value.Value);
            }
        }
    }
}

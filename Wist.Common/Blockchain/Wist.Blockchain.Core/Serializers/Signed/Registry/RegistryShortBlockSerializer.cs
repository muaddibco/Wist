using System.IO;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryShortBlockSerializer : SignatureSupportSerializerBase<RegistryShortBlock>
    {
        public RegistryShortBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_ShortBlock)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.WitnessStateKeys.Length);
            bw.Write((ushort)_block.WitnessUtxoKeys.Length);

            foreach (var item in _block.WitnessStateKeys)
            {
                bw.Write(item.PublicKey.Value.ToArray());
                bw.Write(item.Height);
            }

            foreach (var item in _block.WitnessUtxoKeys)
            {
                bw.Write(item.KeyImage.Value.ToArray());
            }
        }
    }
}

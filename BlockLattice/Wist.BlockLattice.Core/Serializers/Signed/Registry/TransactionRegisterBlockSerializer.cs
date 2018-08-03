using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransactionRegisterBlockSerializer : SyncSupportSerializerBase<TransactionRegisterBlock>
    {
        public TransactionRegisterBlockSerializer(ICryptoService cryptoService) 
            : base(PacketType.Registry, BlockTypes.Registry_TransactionRegister, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.ReferencedPacketType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedHeight);
            bw.Write(_block.ReferencedBodyHash);
        }
    }
}

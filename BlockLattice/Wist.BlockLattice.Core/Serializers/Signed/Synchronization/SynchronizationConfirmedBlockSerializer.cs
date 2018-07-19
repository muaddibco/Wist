using System.IO;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationConfirmedBlockSerializer : SignatureSupportSerializerBase<SynchronizationConfirmedBlock>
    {
        public SynchronizationConfirmedBlockSerializer(ICryptoService cryptoService) 
            : base(PacketType.Synchronization, BlockTypes.Synchronization_ConfirmedBlock, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write(_block.Round);
            bw.Write((byte)(_block.PublicKeys?.Length ?? 0));

            if (_block.PublicKeys != null)
            {
                foreach (byte[] pk in _block.PublicKeys)
                {
                    bw.Write(pk);
                }
            }

            if (_block.Signatures != null)
            {
                foreach (byte[] signature in _block.Signatures)
                {
                    bw.Write(signature);
                }
            }
        }
    }
}

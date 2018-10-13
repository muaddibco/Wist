using System.IO;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationConfirmedBlockSerializer : SyncLinkedSupportSerializerBase<SynchronizationConfirmedBlock>
    {
        public SynchronizationConfirmedBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Synchronization, BlockTypes.Synchronization_ConfirmedBlock, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write(_block.Round);
            byte signersCount = (byte)(_block.PublicKeys?.Length ?? 0);
            bw.Write(signersCount);
            for (int i = 0; i < signersCount; i++)
            {
                bw.Write(_block.PublicKeys[i]);
                bw.Write(_block.Signatures[i]);
            }
        }
    }
}

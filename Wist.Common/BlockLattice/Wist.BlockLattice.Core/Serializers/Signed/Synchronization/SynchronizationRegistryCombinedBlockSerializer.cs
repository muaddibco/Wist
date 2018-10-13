using System.IO;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed.Synchronization
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationRegistryCombinedBlockSerializer : SyncLinkedSupportSerializerBase<SynchronizationRegistryCombinedBlock>
    {
        public SynchronizationRegistryCombinedBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Synchronization, BlockTypes.Synchronization_RegistryCombinationBlock, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.ReportedTime.ToBinary());
            bw.Write((ushort)_block.BlockHashes.Length);
            foreach (byte[] blockHash in _block.BlockHashes)
            {
                bw.Write(blockHash);
            }
        }
    }
}

using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryShortBlockSerializer : SyncSupportSerializerBase<RegistryShortBlock>
    {
        public RegistryShortBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Registry, BlockTypes.Registry_ShortBlock, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.TransactionHeaderHashes.Count);

            foreach (var item in _block.TransactionHeaderHashes)
            {
                bw.Write(item.Key);
                bw.Write(item.Value.Value.ToArray());
            }
        }
    }
}

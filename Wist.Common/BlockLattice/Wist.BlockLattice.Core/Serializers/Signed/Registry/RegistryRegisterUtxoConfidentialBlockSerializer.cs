using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Serializers.UtxoConfidential;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryRegisterUtxoConfidentialBlockSerializer : UtxoConfidentialSerializerBase<RegistryRegisterUtxoConfidentialBlock>
    {
        public RegistryRegisterUtxoConfidentialBlockSerializer(IUtxoConfidentialCryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Registry, BlockTypes.Registry_RegisterUtxoConfidential, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.ReferencedPacketType);
            bw.Write(_block.ReferencedBlockType);
            bw.Write(_block.ReferencedBodyHash);
            bw.Write(_block.ReferencedTarget);
        }
    }
}

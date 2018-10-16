using System.IO;
using Wist.BlockLattice.Core.DataModel;
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
    public class RegistryFullBlockSerializer : SyncSupportSerializerBase<RegistryFullBlock>
    {
        private readonly RegistryRegisterBlockSerializer _transactionRegisterBlockSerializer;

        public RegistryFullBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Registry, BlockTypes.Registry_FullBlock, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
            _transactionRegisterBlockSerializer = new RegistryRegisterBlockSerializer(cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository);
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.TransactionHeaders.Count);

            foreach (var item in _block.TransactionHeaders)
            {
                bw.Write(item.Key);
                _transactionRegisterBlockSerializer.Initialize(item.Value as BlockBase);
                bw.Write(_transactionRegisterBlockSerializer.GetBytes());
            }

            bw.Write(_block.ShortBlockHash);
        }
    }
}

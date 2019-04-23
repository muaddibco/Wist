using System.IO;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class RegistryFullBlockSerializer : SignatureSupportSerializerBase<RegistryFullBlock>
    {
        private readonly RegistryRegisterBlockSerializer _transactionRegisterBlockSerializer;
        private readonly RegistryRegisterUtxoConfidentialBlockSerializer _registryRegisterUtxoConfidentialBlockSerializer;

        public RegistryFullBlockSerializer() : base(PacketType.Registry, BlockTypes.Registry_FullBlock)
        {
            _transactionRegisterBlockSerializer = new RegistryRegisterBlockSerializer();
            _registryRegisterUtxoConfidentialBlockSerializer = new RegistryRegisterUtxoConfidentialBlockSerializer();
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write((ushort)_block.StateWitnesses.Length);
            bw.Write((ushort)_block.UtxoWitnesses.Length);

            foreach (var item in _block.StateWitnesses)
            {
                _transactionRegisterBlockSerializer.Initialize(item);
                bw.Write(_transactionRegisterBlockSerializer.GetBytes());
            }

            foreach (var item in _block.UtxoWitnesses)
            {
                _registryRegisterUtxoConfidentialBlockSerializer.Initialize(item);
                bw.Write(_registryRegisterUtxoConfidentialBlockSerializer.GetBytes());
            }

            bw.Write(_block.ShortBlockHash);
        }
    }
}

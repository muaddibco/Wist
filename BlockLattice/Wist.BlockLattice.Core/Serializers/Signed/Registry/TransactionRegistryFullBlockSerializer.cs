using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransactionRegistryFullBlockSerializer : SyncSupportSerializerBase<RegistryFullBlock>
    {
        private readonly RegistryRegisterBlockSerializer _transactionRegisterBlockSerializer;

        public TransactionRegistryFullBlockSerializer(PacketType packetType, ushort blockType, ICryptoService cryptoService) : base(packetType, blockType, cryptoService)
        {
            _transactionRegisterBlockSerializer = new RegistryRegisterBlockSerializer(cryptoService);
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.TransactionHeaders.Count);

            foreach (var item in _block.TransactionHeaders)
            {
                bw.Write(item.Key);
                _transactionRegisterBlockSerializer.Initialize(item.Value);
                bw.Write(_transactionRegisterBlockSerializer.GetBytes());
            }
        }
    }
}

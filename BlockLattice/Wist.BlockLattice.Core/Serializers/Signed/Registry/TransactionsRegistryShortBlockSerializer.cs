using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers.Signed.Registry
{
    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransactionsRegistryShortBlockSerializer : SyncSupportSerializerBase<RegistryShortBlock>
    {
        public TransactionsRegistryShortBlockSerializer(PacketType packetType, ushort blockType, ICryptoService cryptoService) : base(packetType, blockType, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)_block.TransactionHeaderHashes.Count);

            foreach (var item in _block.TransactionHeaderHashes)
            {
                bw.Write(item.Key);
                bw.Write(item.Value.Value);
            }
        }
    }
}

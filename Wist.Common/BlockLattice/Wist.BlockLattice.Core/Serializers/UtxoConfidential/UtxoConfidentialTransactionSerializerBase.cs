using System.IO;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.UtxoConfidential
{
    public abstract class UtxoConfidentialTransactionSerializerBase<T> : UtxoConfidentialSerializerBase<T> where T : UtxoConfidentialContentBase
    {
        public UtxoConfidentialTransactionSerializerBase(PacketType packetType, ushort blockType, IUtxoConfidentialCryptoService cryptoService, 
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(packetType, blockType, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            _binaryWriter.Write(_block.TransactionPublicKey);
        }
    }
}

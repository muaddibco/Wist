using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed
{
    public abstract class SyncSupportSerializerBase<T> : SignatureSupportSerializerBase<T> where T : SyncedBlockBase
    {
        public SyncSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(packetType, blockType, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.BlockHeight);
        }

        protected override void WriteHeader(BinaryWriter bw)
        {
            base.WriteHeader(bw);

            bw.Write(_block.SyncBlockHeight);
            bw.Write(_block.Nonce);
            bw.Write(_block.PowHash);
        }
    }
}

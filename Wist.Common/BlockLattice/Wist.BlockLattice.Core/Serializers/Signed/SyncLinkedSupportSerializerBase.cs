using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers.Signed
{
    public abstract class SyncLinkedSupportSerializerBase<T> : SyncSupportSerializerBase<T> where T : SyncedLinkedBlockBase
    {
        public SyncLinkedSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(packetType, blockType, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            if (_block.HashPrev == null)
            {
                throw new PreviousHashNotProvidedException();
            }

            bw.Write(_block.HashPrev);
        }
    }
}

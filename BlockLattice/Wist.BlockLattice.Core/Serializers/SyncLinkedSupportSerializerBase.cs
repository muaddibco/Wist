using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.Serializers
{
    public abstract class SyncLinkedSupportSerializerBase<T> : SyncSupportSerializerBase<T> where T : SyncedLinkedBlockBase
    {
        public SyncLinkedSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService) 
            : base(packetType, blockType, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);
            bw.Write(_block.HashPrev);
        }
    }
}

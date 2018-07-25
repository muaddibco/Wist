using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Serializers.Signed;
using Wist.Core.Cryptography;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Serializers
{
    public abstract class SyncSupportSerializerBase<T> : SignatureSupportSerializerBase<T> where T : SyncedBlockBase
    {
        public SyncSupportSerializerBase(PacketType packetType, ushort blockType, ICryptoService cryptoService) 
            : base(packetType, blockType, cryptoService)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.BlockHeight);
            bw.Write(_block.HashPrev);
        }

        protected override void WriteSyncHeader(BinaryWriter bw)
        {
            bw.Write(_block.SyncBlockOrder);
            bw.Write((ushort)_block.POWType);
            if (_block.POWType != Wist.Core.ProofOfWork.POWType.None)
            {
                bw.Write(_block.Nonce);
                bw.Write(_block.HashNonce);
            }
        }
    }
}

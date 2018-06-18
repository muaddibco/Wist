using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;

namespace Wist.BlockLattice.Core.Serializers.Signed
{
    public class SynchronizationProducingBlockSerializer : ISignatureSupportSerializer
    {
        public PacketType PacketType => PacketType.Synchronization;

        public ushort BlockType => BlockTypes.Synchronization_TimeSyncProducingBlock;

        public byte[] GetBody(SignedBlockBase signedBlockBase)
        {
            SynchronizationProducingBlock synchronizationProducingBlock = signedBlockBase as SynchronizationProducingBlock;

            if(synchronizationProducingBlock == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    
                }
            }
        }
    }
}

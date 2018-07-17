using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Cryptography;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Serializers.Signed.Synchronization
{
    public class SynchronizationConfirmedBlockSerializer : SignatureSupportSerializerBase<SynchronizationConfirmedBlock>
    {
        public SynchronizationConfirmedBlockSerializer(ICryptoService cryptoService, IStatesRepository statesRepository, ISignatureSupportSerializersFactory serializersFactory) 
            : base(PacketType.Synchronization, BlockTypes.Synchronization_ConfirmedBlock, cryptoService, statesRepository, serializersFactory)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            
        }

        protected override void WriteSyncHeader(BinaryWriter bw, SynchronizationDescriptor synchronizationDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}

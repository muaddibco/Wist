using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.States;

namespace Wist.BlockLattice.Core.Serializers.Signed
{

    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationProducingBlockSerializer : SignatureSupportSerializerBase<SynchronizationProducingBlock>
    {

        public SynchronizationProducingBlockSerializer(ICryptoService cryptoService, IStatesRepository statesRepository) : base(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncProducingBlock, cryptoService, statesRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            byte[] body = new byte[0];
            byte[] signature;
            byte[] publicKey = _accountState.PublicKey;

            signature = _cryptoService.Sign(body);
        }

        protected override void WritePowHeader(BinaryWriter bw)
        {
            
        }
    }
}

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
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.BlockLattice.Core.Serializers.Signed
{

    [RegisterExtension(typeof(ISignatureSupportSerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SynchronizationProducingBlockSerializer : SignatureSupportSerializerBase<SynchronizationProducingBlock>
    {

        public SynchronizationProducingBlockSerializer(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncProducingBlock, cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.ReportedTime.ToBinary());
        }
    }
}

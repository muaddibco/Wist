using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Synchronization
{
    public abstract class SynchronizationBlockParserBase : SignedBlockParserBase
    {
        public SynchronizationBlockParserBase(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(proofOfWorkCalculationFactory, identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;

        protected override SignedBlockBase ParseSigned(ushort version, BinaryReader br)
        {
            DateTime dateTime = DateTime.FromBinary(br.ReadInt64());

            SynchronizationBlockBase synchronizationBlockBase = ParseSynchronization(version, br);
            synchronizationBlockBase.ReportedTime = dateTime;

            return synchronizationBlockBase;
        }

        protected abstract SynchronizationBlockBase ParseSynchronization(ushort version, BinaryReader br);
    }
}

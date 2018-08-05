using System;
using System.Buffers.Binary;
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
    public abstract class SynchronizationBlockParserBase : SyncLinkedBlockParserBase
    {
        public SynchronizationBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;

        protected override Span<byte> ParseSyncLinked(ushort version, Span<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase)
        {
            DateTime dateTime = DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(spanBody));

            SynchronizationBlockBase synchronizationBlockBase;
            Span<byte> spanPostBody = ParseSynchronization(version, spanBody.Slice(8), out synchronizationBlockBase);
            synchronizationBlockBase.ReportedTime = dateTime;

            syncedBlockBase = synchronizationBlockBase;

            return spanPostBody;
        }

        protected abstract Span<byte> ParseSynchronization(ushort version, Span<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase);
    }
}

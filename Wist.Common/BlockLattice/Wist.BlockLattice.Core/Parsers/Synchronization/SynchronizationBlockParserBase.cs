using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers.Synchronization
{
    public abstract class SynchronizationBlockParserBase : SyncLinkedBlockParserBase
    {
        public SynchronizationBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) 
            : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;

        protected override Memory<byte> ParseSyncLinked(ushort version, Memory<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase)
        {
            DateTime dateTime = DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(spanBody.Span));

            Memory<byte> spanPostBody = ParseSynchronization(version, spanBody.Slice(8), out SynchronizationBlockBase synchronizationBlockBase);
            synchronizationBlockBase.ReportedTime = dateTime;

            syncedBlockBase = synchronizationBlockBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase);
    }
}

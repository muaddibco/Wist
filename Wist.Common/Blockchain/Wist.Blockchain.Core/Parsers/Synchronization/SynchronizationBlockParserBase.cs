using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Synchronization
{
    public abstract class SynchronizationBlockParserBase : LinkedBlockParserBase
    {
        public SynchronizationBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;

        protected override Memory<byte> ParseLinked(ushort version, Memory<byte> spanBody, out LinkedPacketBase syncedBlockBase)
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

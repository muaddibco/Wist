using System;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncLinkedBlockParserBase : SyncedBlockParserBase
    {
        public SyncLinkedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) 
            : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
        }

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase signedBlockBase)
        {
            byte[] prevHash = spanBody.Slice(0, Globals.HASH_SIZE).ToArray();
            SyncedLinkedBlockBase syncedBlockBase;
            Span<byte> spanPostBody = ParseSyncLinked(version, spanBody.Slice(Globals.HASH_SIZE), out syncedBlockBase);
            syncedBlockBase.HashPrev = prevHash;
            signedBlockBase = syncedBlockBase;

            return spanPostBody;
        }

        protected abstract Span<byte> ParseSyncLinked(ushort version, Span<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase);
    }
}

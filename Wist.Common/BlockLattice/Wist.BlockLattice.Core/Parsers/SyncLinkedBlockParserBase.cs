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

        protected override Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase signedBlockBase)
        {
            byte[] prevHash = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray();
            Memory<byte> spanPostBody = ParseSyncLinked(version, spanBody.Slice(Globals.DEFAULT_HASH_SIZE), out SyncedLinkedBlockBase syncedBlockBase);
            syncedBlockBase.HashPrev = prevHash;
            signedBlockBase = syncedBlockBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseSyncLinked(ushort version, Memory<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase);
    }
}

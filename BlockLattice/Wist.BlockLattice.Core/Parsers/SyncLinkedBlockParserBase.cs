using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncLinkedBlockParserBase : SyncedBlockParserBase
    {
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;

        public SyncLinkedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
        }

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase signedBlockBase)
        {
            byte[] prevHash = spanBody.Slice(8, Globals.HASH_SIZE).ToArray();
            SyncedLinkedBlockBase syncedBlockBase;
            Span<byte> spanPostBody = ParseSyncLinked(version, spanBody.Slice(8 + Globals.HASH_SIZE), out syncedBlockBase);
            syncedBlockBase.HashPrev = prevHash;
            signedBlockBase = syncedBlockBase;

            return spanPostBody;
        }

        protected abstract Span<byte> ParseSyncLinked(ushort version, Span<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase);
    }
}

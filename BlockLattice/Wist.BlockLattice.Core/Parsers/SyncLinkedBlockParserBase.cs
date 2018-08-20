﻿using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncLinkedBlockParserBase : SyncedBlockParserBase
    {
        private readonly IHashCalculationRepository _proofOfWorkCalculationRepository;

        public SyncLinkedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationRepository proofOfWorkCalculationRepository) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
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

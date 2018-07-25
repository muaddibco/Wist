﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : SyncedBlockParserBase
    {
        private readonly IProofOfWorkCalculationRepository _proofOfWorkCalculationRepository;

        public TransactionalBlockParserBase(IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
        }

        public override PacketType PacketType => PacketType.TransactionalChain;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            TransactionalBlockBase transactionalBlockBase;

            Span<byte> spanPostBody = ParseTransactional(version, spanBody, out transactionalBlockBase);
            syncedBlockBase = transactionalBlockBase;

            return spanPostBody;
        }

        protected abstract Span<byte> ParseTransactional(ushort version, Span<byte> spanBody, out TransactionalBlockBase transactionalBlockBase);
    }
}

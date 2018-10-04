using System;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : SyncLinkedBlockParserBase
    {
        private readonly IHashCalculationsRepository _proofOfWorkCalculationRepository;

        public TransactionalBlockParserBase(IHashCalculationsRepository proofOfWorkCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry, proofOfWorkCalculationRepository)
        {
            _proofOfWorkCalculationRepository = proofOfWorkCalculationRepository;
        }

        public override PacketType PacketType => PacketType.TransactionalChain;

        protected override Memory<byte> ParseSyncLinked(ushort version, Memory<byte> spanBody, out SyncedLinkedBlockBase syncedBlockBase)
        {

            Memory<byte> spanPostBody = ParseTransactional(version, spanBody, out TransactionalBlockBase transactionalBlockBase);
            syncedBlockBase = transactionalBlockBase;

            return spanPostBody;
        }

        protected abstract Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalBlockBase transactionalBlockBase);
    }
}

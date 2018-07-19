using System;
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
        private readonly IProofOfWorkCalculationFactory _proofOfWorkCalculationFactory;

        public TransactionalBlockParserBase(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
            _proofOfWorkCalculationFactory = proofOfWorkCalculationFactory;
        }

        public override PacketType PacketType => PacketType.TransactionalChain;

        protected override SyncedBlockBase ParseSynced(ushort version, BinaryReader br)
        {
            TransactionalBlockBase transactionalBlockBase =  ParseTransactional(version, br);

            return transactionalBlockBase;
        }

        protected override void ReadPowSection(BinaryReader br)
        {
            POWType powType = (POWType)br.ReadUInt16();
            br.ReadUInt32();
            br.ReadUInt64();
            br.ReadBytes(_proofOfWorkCalculationFactory.Create(powType).HashSize);
        }

        protected abstract TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br);
    }
}

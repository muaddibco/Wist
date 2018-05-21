using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : BlockParserBase
    {
        public TransactionalBlockParserBase(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(proofOfWorkCalculationFactory)
        {
        }

        public override ChainType ChainType => ChainType.TransactionalChain;

        protected override BlockBase Parse(ushort version, BinaryReader br)
        {
            byte[] originalHash = br.ReadBytes(Globals.HASH_SIZE);
            byte[] nbackedHash = br.ReadBytes(Globals.HASH_SIZE);
            TransactionalBlockBase transactionalBlockBase =  ParseTransactional(version, br);
            transactionalBlockBase.OriginalHash = originalHash;
            transactionalBlockBase.NBackHash = nbackedHash;

            return transactionalBlockBase;
        }

        protected abstract TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br);
    }
}

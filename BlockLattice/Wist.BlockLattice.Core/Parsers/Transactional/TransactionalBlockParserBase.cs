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

        public override PacketType PacketType => PacketType.TransactionalChain;

        protected override BlockBase Parse(ushort version, BinaryReader br)
        {
            TransactionalBlockBase transactionalBlockBase =  ParseTransactional(version, br);

            return transactionalBlockBase;
        }

        protected abstract TransactionalBlockBase ParseTransactional(ushort version, BinaryReader br);
    }
}

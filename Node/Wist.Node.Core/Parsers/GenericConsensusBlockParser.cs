using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Node.Core.Parsers
{
    public class GenericConsensusBlockParser : ConsensusBlockParserBase
    {
        public GenericConsensusBlockParser(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(proofOfWorkCalculationFactory)
        {
        }

        public override ushort BlockType => BlockTypes.Consensus_GenericConsensus;

        public override void FillBlockBody(BlockBase block, byte[] blockBody)
        {
            throw new NotImplementedException();
        }

        protected override BlockBase Parse(ushort version, BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}

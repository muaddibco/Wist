using System;
using System.IO;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Node.Core.Parsers
{
    public class GenericConsensusBlockParser : ConsensusBlockParserBase
    {
        public GenericConsensusBlockParser(IProofOfWorkCalculationRepository proofOfWorkCalculationRepository) : base(proofOfWorkCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Consensus_GenericConsensus;

        protected override BlockBase ParseBlockBase(ushort version, Span<byte> spanBody)
        {
            throw new NotImplementedException();
        }
    }
}

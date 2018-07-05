using System;
using System.IO;
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

        protected override BlockBase Parse(ushort version, ulong height, byte[] prevHash, BinaryReader br)
        {
            throw new NotImplementedException();
        }
    }
}

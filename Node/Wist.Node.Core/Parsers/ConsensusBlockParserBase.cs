using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers;
using Wist.Core.ProofOfWork;

namespace Wist.Node.Core.Parsers
{
    public abstract class ConsensusBlockParserBase : BlockParserBase
    {
        public ConsensusBlockParserBase(IProofOfWorkCalculationFactory proofOfWorkCalculationFactory) : base(proofOfWorkCalculationFactory)
        {
        }

        public override PacketType PacketType => PacketType.Consensus;
    }
}

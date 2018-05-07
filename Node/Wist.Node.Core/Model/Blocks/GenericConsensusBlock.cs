using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core.Model.Blocks
{
    public class GenericConsensusBlock : ConsensusBlockBaseV1
    {
        public override ushort BlockType => BlockTypes.Consensus_GenericConsensus;

        public BlockBase Block { get; set; }

        public ConsensusDecisionItem[] ConsensusDecisions { get; set; }

        public class ConsensusDecisionItem
        {
            public byte[] PublickKey { get; set; }
            public ConsensusState ConsensusState { get; set; }
        }
    }
}

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

        public byte[] PublickKey { get; set; }

        public BlockBase Block { get; set; }

        public ConsensusState ConsensusState { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core
{
    public class BlockConsensusState
    {
        public BlockConsensusState(BlockBase block, Dictionary<string, ConsensusState> decisionMap)
        {
            Block = block;
            ParticipantDecisionsMap = decisionMap;
            IsChecked = false;
            IsConsensusReached = false;
        }

        public BlockBase Block { get; }

        public Dictionary<string, ConsensusState> ParticipantDecisionsMap { get; }

        public bool IsChecked { get; set; }
        public bool IsConsensusReached { get; set; }
    }
}

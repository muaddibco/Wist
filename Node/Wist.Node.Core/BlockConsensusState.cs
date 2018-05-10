using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core
{
    public class BlockConsensusState
    {
        public BlockConsensusState(BlockBase block, Dictionary<string, ValidationState> decisionMap)
        {
            Block = block;
            ParticipantDecisionsMap = decisionMap;
            IsChecked = false;
            IsConsensusReached = false;
        }

        public BlockBase Block { get; }

        public Dictionary<string, ValidationState> ParticipantDecisionsMap { get; }

        public bool IsChecked { get; set; }
        public bool IsConsensusReached { get; set; }
    }
}

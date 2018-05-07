using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core.Model
{
    public class ConsensusDecision
    {
        public ConsensusGroupParticipant Participant { get; set; }

        public ConsensusState State { get; set; }
    }
}

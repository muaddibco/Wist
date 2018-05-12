using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core.Model
{
    public class ValidationDecision
    {
        public ConsensusGroupParticipant Participant { get; set; }

        public ValidationState State { get; set; }
    }
}

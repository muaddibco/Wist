using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Node.Core.Model
{
    public class ConsensusGroupParticipant
    {
        /// <summary>
        /// 32 byte length Public Key of Consensus Group Participant
        /// </summary>
        public byte[] PublicKey { get; set; }

        public double Weight { get; set; }
    }
}

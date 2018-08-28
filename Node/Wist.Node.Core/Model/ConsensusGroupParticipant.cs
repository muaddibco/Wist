using Wist.Core.Identity;

namespace Wist.Node.Core.Model
{
    public class ConsensusGroupParticipant
    {
        /// <summary>
        /// 32 byte length Public Key of Consensus Group Participant
        /// </summary>
        public IKey PublicKey { get; set; }

        public double Weight { get; set; }
    }
}

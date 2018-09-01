using System.Collections.Generic;
using System.Threading;
using Wist.BlockLattice.Core.DataModel.Registry;

namespace Wist.Node.Core.Synchronization
{
    public class RoundDescriptor
    {
        public RoundDescriptor(Timer timer)
        {
            CandidateBlocks = new Dictionary<RegistryFullBlock, int>();
            VotingBlocks = new HashSet<RegistryConfidenceBlock>();
            Timer = timer;
        }
        
        public byte Round { get; set; }
        public Dictionary<RegistryFullBlock, int> CandidateBlocks { get; }
        public HashSet<RegistryConfidenceBlock> VotingBlocks { get; }
        public Timer Timer { get; }
        public bool IsFinished { get; set; }
    }
}

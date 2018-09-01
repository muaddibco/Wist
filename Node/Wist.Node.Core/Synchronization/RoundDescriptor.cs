using System.Collections.Generic;
using System.Threading;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Identity;

namespace Wist.Node.Core.Synchronization
{
    public class RoundDescriptor
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public RoundDescriptor(Timer timer, IIdentityKeyProvider identityKeyProvider)
        {
            CandidateBlocks = new Dictionary<IKey, RegistryFullBlock>();
            CandidateVotes = new Dictionary<IKey, int>();
            VotingBlocks = new HashSet<RegistryConfidenceBlock>();
            Timer = timer;
            _identityKeyProvider = identityKeyProvider;
        }
        
        public byte Round { get; set; }
        public Dictionary<IKey, RegistryFullBlock> CandidateBlocks { get; }
        public Dictionary<IKey, int> CandidateVotes { get; }
        public HashSet<RegistryConfidenceBlock> VotingBlocks { get; }
        public Timer Timer { get; }
        public bool IsFinished { get; set; }

        public void AddFullBlock(RegistryFullBlock registryFullBlock)
        {
            IKey key = _identityKeyProvider.GetKey(registryFullBlock.ShortBlockHash);
            if (!CandidateBlocks.ContainsKey(key))
            {
                CandidateBlocks.Add(key, registryFullBlock);
                CandidateVotes.Add(key, 0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Node.Core.DPOS
{
    [RegisterExtension(typeof(INodeDposProvider), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDposContributionProvider : INodeDposProvider
    {
        private readonly IChainDataService _chainDataService;

        private Dictionary<IKey, DposDescriptor> _dposDescriptors = new Dictionary<IKey, DposDescriptor>();
        private Dictionary<IKey, List<DposDescriptor>> _votesForCandidates;

        public PacketType PacketType => PacketType.TransactionalChain;

        public TransactionalChainDposContributionProvider(IChainDataServicesManager chainDataServicesManager)
        {
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.TransactionalChain);
        }

        public double GetVotesForCandidate(IKey nodeKey)
        {
            return _votesForCandidates.ContainsKey(nodeKey) ? _votesForCandidates[nodeKey].Count() : 0;
        }

        public void Initialize()
        {
            //TODO: need to take all last blocks where DPOS delegation is specified
            IEnumerable<TransactionalGenesisBlock> genesisBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalGenesisBlock>();
            //IEnumerable<TransactionalBlockBaseV1> transactionalBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalBlockBaseV1>();

            _dposDescriptors = genesisBlocks.ToDictionary(g => g.Signer, g => new DposDescriptor { SourceIdentity = g.Signer, TargetIdentity = g.NodeDpos, Votes = 1 });
            _votesForCandidates = _dposDescriptors.GroupBy(d => d.Value.TargetIdentity).ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());
            //genesisBlocks.GroupBy(b => b.NodeDpos).ToDictionary(g => g.Key, g => new DposDescriptor {SourceIdentity = g.Key, TargetIdentity =  g.Sum(b => transactionalBlocks.FirstOrDefault(t => t.Key == b.Key)?.UptodateFunds??0)
        }

        public void UpdateContribution(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public int GetCandidateRating(IKey candidateKey)
        {
            int index = _votesForCandidates.OrderByDescending(cv => cv.Value.Count()).OrderByDescending(cv => cv.Key.GetHashCode()).Select(cv => cv.Key).ToList().IndexOf(candidateKey);

            return index;
        }
    }
}

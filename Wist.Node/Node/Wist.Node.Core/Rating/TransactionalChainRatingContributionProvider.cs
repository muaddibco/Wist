using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Node.Core.Rating
{
    [RegisterExtension(typeof(INodesRatingProvider), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainRatingContributionProvider : INodesRatingProvider
    {
        private readonly IChainDataService _chainDataService;
        private readonly INodesDataService _nodesDataService;
        private readonly List<IKey> _topNodeKeys;
        private Dictionary<IKey, DposDescriptor> _dposDescriptors = new Dictionary<IKey, DposDescriptor>();
        private Dictionary<IKey, List<DposDescriptor>> _votesForCandidates;

        public PacketType PacketType => PacketType.Transactional;

        public TransactionalChainRatingContributionProvider(IChainDataServicesManager chainDataServicesManager, INodesDataService nodesDataService)
        {
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.Transactional);
            _nodesDataService = nodesDataService;
            _topNodeKeys = new List<IKey>();
        }

        public double GetVotesForCandidate(IKey nodeKey)
        {
            return _votesForCandidates.ContainsKey(nodeKey) ? _votesForCandidates[nodeKey].Count() : 0;
        }

        public void Initialize()
        {
            _topNodeKeys.AddRange(_nodesDataService.GetAll().Where(n => n.NodeRole == NodeRole.SynchronizationLayer).Take(21).Select(n => n.Key));
            ////TODO: need to take all last blocks where DPOS delegation is specified
            //IEnumerable<TransactionalGenesisBlock> genesisBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalGenesisBlock>();
            ////IEnumerable<TransactionalBlockBaseV1> transactionalBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalBlockBaseV1>();

            //_dposDescriptors = genesisBlocks.ToDictionary(g => g.Signer, g => new DposDescriptor { SourceIdentity = g.Signer, TargetIdentity = g.NodeDpos, Votes = 1 });
            //_votesForCandidates = _dposDescriptors.GroupBy(d => d.Value.TargetIdentity).ToDictionary(g => g.Key, g => g.Select(v => v.Value).ToList());
            ////genesisBlocks.GroupBy(b => b.NodeDpos).ToDictionary(g => g.Key, g => new DposDescriptor {SourceIdentity = g.Key, TargetIdentity =  g.Sum(b => transactionalBlocks.FirstOrDefault(t => t.Key == b.Key)?.UptodateFunds??0)
        }

        public void UpdateContribution(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public int GetCandidateRating(IKey candidateKey)
        {
            //int index = _votesForCandidates.OrderByDescending(cv => cv.Value.Count()).OrderByDescending(cv => cv.Key.GetHashCode()).Select(cv => cv.Key).ToList().IndexOf(candidateKey);
            int index = _topNodeKeys.FindIndex(k => candidateKey.Equals(k));
            return index;
        }

        public bool IsCandidateInTopList(IKey candidateKey) => _topNodeKeys.Contains(candidateKey);
        public int GetParticipantsCount() => _topNodeKeys.Count;
    }
}

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
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.DPOS
{
    [RegisterExtension(typeof(INodeDposProvider), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDposContributionProvider : INodeDposProvider
    {
        private readonly IChainDataService _chainDataService;

        private Dictionary<IKey, DposDescriptor> _dposDescriptors = new Dictionary<IKey, DposDescriptor>();

        public PacketType ChainType => PacketType.TransactionalChain;

        public TransactionalChainDposContributionProvider(IChainDataServicesManager chainDataServicesManager)
        {
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.TransactionalChain);
        }

        public double GetAllContributions(byte[] nodePublicKey)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            //TODO: need to take all last blocks where DPOS delegation is specified
            IEnumerable<TransactionalGenesisBlock> genesisBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalGenesisBlock>();
            IEnumerable<TransactionalBlockBaseV1> transactionalBlocks = _chainDataService.GetAllLastBlocksByType<TransactionalBlockBaseV1>();

            _dposDescriptors = genesisBlocks.ToDictionary(g => g.Key, g => new DposDescriptor { SourceIdentity = g.Key, TargetIdentity = g.NodeDpos, Votes = transactionalBlocks.FirstOrDefault(t => t.Key == g.Key)?.UptodateFunds ?? 0 });
            //genesisBlocks.GroupBy(b => b.NodeDpos).ToDictionary(g => g.Key, g => new DposDescriptor {SourceIdentity = g.Key, TargetIdentity =  g.Sum(b => transactionalBlocks.FirstOrDefault(t => t.Key == b.Key)?.UptodateFunds??0)
        }

        public void UpdateContribution(BlockBase block)
        {
            throw new NotImplementedException();
        }
    }
}

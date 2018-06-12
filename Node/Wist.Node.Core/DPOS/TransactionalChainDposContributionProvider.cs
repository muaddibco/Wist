using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.DPOS
{
    [RegisterExtension(typeof(INodeDposProvider), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDposContributionProvider : INodeDposProvider
    {
        private readonly IChainDataService _chainDataService;

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
            List<BlockBase> blocks = _chainDataService.GetAllLastBlocksByType(BlockTypes.Transaction_Genesis);
        }

        public void UpdateContribution(BlockBase block)
        {
            throw new NotImplementedException();
        }
    }
}

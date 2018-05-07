using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model.Blocks;
using Wist.Node.Core.Properties;

namespace Wist.Node.Core.ConsensusServices
{
    [RegisterExtension(typeof(IChainConsensusService), Lifetime = LifetimeManagement.Singleton)]
    public class DecisionConsensusService : IChainConsensusService
    {
        public ChainType ChainType => ChainType.Consensus;
        private IReportConsensus _reportConsensus;
        private IChainConsensusServiceManager _chainConsensusServiceManager;

        public void EnrollForConsensus(BlockBase block)
        {
            // TODO: Make processing in separate long running task

            GenericConsensusBlock consensusBlock = block as GenericConsensusBlock;

            if(consensusBlock == null)
            {
                throw new ArgumentOutOfRangeException(nameof(block), string.Format(Resources.ERR_UNEXPECTED_ARGUMENT_TYPE, typeof(GenericConsensusBlock)));
            }

            IChainConsensusService consensusService = _chainConsensusServiceManager.GetChainConsensysService(consensusBlock.Block.ChainType);

            if(!consensusService.IsBlockEnrolled(consensusBlock.Block))
            {
                consensusService.EnrollForConsensus(consensusBlock.Block);
            }
        }

        public void Initialize(IReportConsensus reportConsensus, CancellationToken cancellationToken)
        {
            _reportConsensus = reportConsensus;
            _chainConsensusServiceManager = ServiceLocator.Current.GetInstance<IChainConsensusServiceManager>();
        }

        public bool IsBlockEnrolled(BlockBase block)
        {
            return false;
        }
    }
}

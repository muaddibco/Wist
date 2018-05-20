using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model.Blocks;
using Wist.Node.Core.Properties;

namespace Wist.Node.Core.ValidationServices
{
    [RegisterExtension(typeof(IChainValidationService), Lifetime = LifetimeManagement.Singleton)]
    public class DecisionValidationService : IChainValidationService
    {
        public ChainType ChainType => ChainType.Consensus;
        private IConsumeValidationReport _reportConsensus;
        private IChainValidationServiceManager _chainConsensusServiceManager;
        private readonly IConsensusHub _consensusHub;

        public DecisionValidationService(IConsensusHub consensusHub)
        {
            _consensusHub = consensusHub;
        }

        public void EnrollForConsensus(BlockBase block)
        {
            //TODO: Make processing in separate long running task

            GenericConsensusBlock consensusBlock = block as GenericConsensusBlock;

            if(consensusBlock == null)
            {
                throw new ArgumentOutOfRangeException(nameof(block), string.Format(Resources.ERR_UNEXPECTED_ARGUMENT_TYPE, typeof(GenericConsensusBlock)));
            }

            IChainValidationService consensusService = _chainConsensusServiceManager.GetChainValidationService(consensusBlock.Block.ChainType);

            if(!consensusService.IsBlockEnrolled(consensusBlock.Block))
            {
                consensusService.EnrollForConsensus(consensusBlock.Block);
            }

            _reportConsensus.OnValidationReport(consensusBlock.Block, consensusBlock.ConsensusDecisions.Select(c => new Model.ValidationDecision() { Participant = _consensusHub.GroupParticipants[c.PublickKey], State = c.ConsensusState }));
        }

        public void Initialize(IConsumeValidationReport reportConsensus, CancellationToken cancellationToken)
        {
            _reportConsensus = reportConsensus;
            _chainConsensusServiceManager = ServiceLocator.Current.GetInstance<IChainValidationServiceManager>();
        }

        public bool IsBlockEnrolled(BlockBase block)
        {
            return false;
        }
    }
}

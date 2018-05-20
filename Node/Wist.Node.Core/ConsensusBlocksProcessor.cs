using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;
using Wist.Node.Core.Model.Blocks;

namespace Wist.Node.Core
{
    [RegisterExtension(typeof(IBlocksProcessor), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlocksProcessor : IBlocksProcessor, IConsumeValidationReport, IRequiresCommunicationHub
    {
        public const string BLOCKS_PROCESSOR_NAME = "ConsensusBlocksProcessor";

        private CancellationToken _cancellationToken;

        private readonly object _sync = new object();
        private readonly INodeContext _nodeContext;
        private readonly IChainValidationServiceManager _chainConsensusServiceManager;

        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _blocks;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _locallyApproved;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _consensusAchievedBlocks;
        private readonly IConsensusCheckingService _consensusCheckingService;
        private readonly BlockingCollection<GenericConsensusBlock> _consensusItems; //TODO: need to decide how to know, that decision must be retransmitted

        private ICommunicationHub _communicationHub;
        private bool _isInitialized;

        public string Name => BLOCKS_PROCESSOR_NAME;

        public ConsensusBlocksProcessor(INodeContext nodeContext, IChainValidationServiceManager chainConsensusServiceManager, IConsensusCheckingService consensusCheckingService)
        {
            _nodeContext = nodeContext;
            _chainConsensusServiceManager = chainConsensusServiceManager;
            _consensusCheckingService = consensusCheckingService;
            _blocks = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _locallyApproved = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _consensusAchievedBlocks = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _consensusItems = new BlockingCollection<GenericConsensusBlock>();

            foreach (var chainType in Enum.GetValues(typeof(ChainType)))
            {
                _blocks.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
                _locallyApproved.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
                _consensusAchievedBlocks.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
            }
        }

        public void RegisterCommunicationHub(ICommunicationHub communicationHub)
        {
            _communicationHub = communicationHub;
        }

        public void Initialize(CancellationToken ct)
        {
            //TODO: add exception AlreadyInitialized
            if (_isInitialized)
                return;

            lock (_sync)
            {
                if (_isInitialized)
                    return;

                _cancellationToken = ct;

                foreach (ChainType chainType in Enum.GetValues(typeof(ChainType)))
                {
                    IChainValidationService chainConsensysService = _chainConsensusServiceManager.GetChainValidationService(chainType);
                    chainConsensysService.Initialize(this, ct);

                    Task.Factory.StartNew(o =>
                    {
                        Tuple<ChainType, CancellationToken> inputArgs = (Tuple<ChainType, CancellationToken>)o;

                        ProcessBlocks(_blocks[inputArgs.Item1], inputArgs.Item2);
                    }, new Tuple<ChainType, CancellationToken>( chainType, ct), TaskCreationOptions.LongRunning);
                }

                _consensusCheckingService.Initialize(ct);

                _isInitialized = true;
            }
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            //TODO: add exception NotInitialized
            if (!_isInitialized)
                return;

            BlockBase blockToProcess = blockBase;

            if (blockBase is GenericConsensusBlock)
            {
                blockToProcess = ((GenericConsensusBlock)blockBase).Block;
            }

            if (_blocks.ContainsKey(blockBase.ChainType))
            {
                _blocks[blockBase.ChainType].Enqueue(blockBase);
            }
        }

        public void OnValidationReport(BlockBase block, IEnumerable<ValidationDecision> consensusDecisions)
        {
            _consensusCheckingService.EnrollConsensusDecisions(block, consensusDecisions);

            if (consensusDecisions.Any(p => p.Participant.PublicKey.Equals32(_nodeContext.PublicKey)))
            {
                _consensusItems.Add(new GenericConsensusBlock() { Block = block, ConsensusDecisions = consensusDecisions.Select(d => new GenericConsensusBlock.ConsensusDecisionItem { PublickKey = d.Participant.PublicKeyString, ConsensusState = d.State }).ToArray() });
            }
        }

        #region Private Functions


        private void ProcessBlocks(ConcurrentQueue<BlockBase> blocks, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BlockBase blockBase;

                if (blocks.TryDequeue(out blockBase))
                {
                    //TODO: need to understand whether consensus on blocks must be reached sequentially or it can be done in parallel
                    IChainValidationService chainConsensysService = _chainConsensusServiceManager.GetChainValidationService(blockBase.ChainType);

                    chainConsensysService.EnrollForConsensus(blockBase);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        private void RetransmitBloksConsensus(CancellationToken cancellationToken)
        {
            foreach (var item in _consensusItems.GetConsumingEnumerable(cancellationToken))
            {
                _communicationHub.BroadcastMessage(item);
            }
        }

        #endregion Private Functions
    }
}

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
    [RegisterDefaultImplementation(typeof(IBlocksProcessor), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlocksProcessor : IBlocksProcessor, IReportConsensus
    {
        private CancellationToken _cancellationToken;

        private readonly object _sync = new object();
        private readonly INodeContext _nodeContext;
        private readonly IChainConsensusServiceManager _chainConsensusServiceManager;
        private bool _isInitialized;

        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _blocks;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _locallyApproved;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _consensusAchievedBlocks;
        private readonly ICommunicationHub _communicationHub;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private readonly BlockingCollection<GenericConsensusBlock> _consensusItems; // TODO: need to decide how to know, that decision must be retransmitted
        private readonly ConcurrentDictionary<BlockBase, BlockConsensusState> _blockConsensusStatesMap;
        private readonly ConcurrentQueue<BlockConsensusState> _blockConsensusToCheck;


        public ConsensusBlocksProcessor(INodeContext nodeContext, IChainConsensusServiceManager chainConsensusServiceManager, ICommunicationHub communicationHub, IChainDataServicesManager chainDataServicesManager)
        {
            _nodeContext = nodeContext;
            _chainConsensusServiceManager = chainConsensusServiceManager;
            _communicationHub = communicationHub;
            _chainDataServicesManager = chainDataServicesManager;
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

            _blockConsensusStatesMap = new ConcurrentDictionary<BlockBase, BlockConsensusState>();
            _blockConsensusToCheck = new ConcurrentQueue<BlockConsensusState>();
        }

        public void Initialize(CancellationToken ct)
        {
            // TODO: add exception AlreadyInitialized
            if (_isInitialized)
                return;

            lock (_sync)
            {
                if (_isInitialized)
                    return;

                _cancellationToken = ct;

                foreach (ChainType chainType in Enum.GetValues(typeof(ChainType)))
                {
                    IChainConsensusService chainConsensysService = _chainConsensusServiceManager.GetChainConsensysService(chainType);
                    chainConsensysService.Initialize(this, ct);

                    Task.Factory.StartNew(o =>
                    {
                        Tuple<ChainType, CancellationToken> inputArgs = (Tuple<ChainType, CancellationToken>)o;

                        ProcessBlocks(_blocks[inputArgs.Item1], inputArgs.Item2);
                    }, new Tuple<ChainType, CancellationToken>( chainType, ct), TaskCreationOptions.LongRunning);
                }

                _isInitialized = true;
            }
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            // TODO: add exception NotInitialized
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

        public void OnReportConsensus(BlockBase block, IEnumerable<ConsensusDecision> consensusDecisions)
        {
            UpdateConsensusDecision(block, consensusDecisions);
        }

        #region Private Functions

        private void UpdateConsensusDecision(BlockBase block, IEnumerable<ConsensusDecision> consensusDecisions)
        {
            BlockConsensusState consensusState;

            if (!_blockConsensusStatesMap.ContainsKey(block))
            {
                consensusState = new BlockConsensusState(block, consensusDecisions.ToDictionary(c => c.Participant.PublicKey.ToHexString(), c => c.State));
                _blockConsensusStatesMap.TryAdd(block, consensusState);
            }
            else if (_blockConsensusStatesMap.TryGetValue(block, out consensusState))
            {
                lock (consensusState)
                {
                    foreach (var consensusDecision in consensusDecisions.Where(d => !consensusState.ParticipantDecisionsMap.ContainsKey(d.Participant.PublicKey.ToHexString())))
                    {
                        consensusState.IsChecked = false;
                        consensusState.ParticipantDecisionsMap.Add(consensusDecision.Participant.PublicKey.ToHexString(), consensusDecision.State);

                        if (consensusDecision.Participant.PublicKey.Equals32(_nodeContext.PublicKey))
                        {
                            _consensusItems.Add(new GenericConsensusBlock() { Block = block, ConsensusDecisions = consensusDecisions.Select(d => new GenericConsensusBlock.ConsensusDecisionItem { PublickKey = d.Participant.PublicKey, ConsensusState = d.State }).ToArray() });
                        }
                    }
                }

                if (!consensusState.IsChecked && !consensusState.IsConsensusReached)
                {
                    _blockConsensusToCheck.Enqueue(consensusState);
                }
            }
        }

        private void CheckConsensusDecisions(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                BlockConsensusState blockConsensusState;
                if(_blockConsensusToCheck.TryDequeue(out blockConsensusState))
                {
                    CheckDecisions(blockConsensusState);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        private void CheckDecisions(BlockConsensusState blockConsensusState)
        {
            bool approved;
            bool consensusReached = false;
            lock (blockConsensusState)
            {
                consensusReached = ConsensusReached(blockConsensusState, out approved);
                if(consensusReached)
                {
                    lock (_blockConsensusStatesMap)
                    {
                        BlockConsensusState removedBlockConsensusState;
                        _blockConsensusStatesMap.TryRemove(blockConsensusState.Block, out removedBlockConsensusState);
                    }
                }

                blockConsensusState.IsChecked = true;
                blockConsensusState.IsChecked = consensusReached;
            }

            if(approved)
            {
                IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService(blockConsensusState.Block.ChainType);
                chainDataService.AddBlock(blockConsensusState.Block);
            }
        }

        private bool ConsensusReached(BlockConsensusState blockConsensusState, out bool approved)
        {
            // TODO: implement real check
            approved = true;
            return true;
        }

        private void ProcessBlocks(ConcurrentQueue<BlockBase> blocks, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BlockBase blockBase;

                if (blocks.TryDequeue(out blockBase))
                {
                    // TODO: need to understand whether consensus on blocks must be reached sequentially or it can be done in parallel
                    IChainConsensusService chainConsensysService = _chainConsensusServiceManager.GetChainConsensysService(blockBase.ChainType);

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

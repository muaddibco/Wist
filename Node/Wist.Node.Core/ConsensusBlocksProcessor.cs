using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model.Blocks;

namespace Wist.Node.Core
{
    [RegisterDefaultImplementation(typeof(IBlocksProcessor), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlocksProcessor : IBlocksProcessor, IReportConsensus
    {
        private CancellationToken _cancellationToken;

        private readonly object _sync = new object();
        private readonly IChainConsensusServiceManager _chainConsensusServiceManager;
        private bool _isInitialized;

        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _blocks;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _locallyApproved;
        private readonly BlockingCollection<GenericConsensusBlock> _consensusItems;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _consensusAchievedBlocks;
        private readonly ICommunicationHub _communicationHub;

        public ConsensusBlocksProcessor(IChainConsensusServiceManager chainConsensusServiceManager, ICommunicationHub communicationHub)
        {
            _chainConsensusServiceManager = chainConsensusServiceManager;
            _communicationHub = communicationHub;
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

        public void OnReportConsensus(BlockBase block, ConsensusState consensusState)
        {
            switch (consensusState)
            {
                case ConsensusState.Approved:
                case ConsensusState.Rejected:
                    _consensusItems.Add(new GenericConsensusBlock { Block = block, ConsensusState = consensusState});
                    break;
                default:
                    break;
            }
        }

        #region Private Functions

        private bool CheckBlockIsNotEnrolledYet(BlockBase block)
        {

        }

        private void ProcessBlocks(ConcurrentQueue<BlockBase> blocks, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BlockBase blockBase;

                if (blocks.TryDequeue(out blockBase))
                {
                    // TODO: need to understand whether consensus on blocks must be reasched sequentially or it can be done in parallel
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Enums;
using Wist.Node.Core.Interfaces;

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
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _locallyRejected;
        private readonly Dictionary<ChainType, ConcurrentQueue<BlockBase>> _consensusAchievedBlocks;

        public ConsensusBlocksProcessor(IChainConsensusServiceManager chainConsensusServiceManager)
        {
            _chainConsensusServiceManager = chainConsensusServiceManager;
            _blocks = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _locallyApproved = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _locallyRejected = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();
            _consensusAchievedBlocks = new Dictionary<ChainType, ConcurrentQueue<BlockBase>>();

            foreach (var chainType in Enum.GetValues(typeof(ChainType)))
            {
                _blocks.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
                _locallyApproved.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
                _locallyRejected.Add((ChainType)chainType, new ConcurrentQueue<BlockBase>());
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

                        ProcessBlocks(_blocks[inputArgs.Item1], _locallyApproved[inputArgs.Item1], _locallyRejected[inputArgs.Item1], inputArgs.Item2);
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

                    break;
                case ConsensusState.Rejected:
                    _locallyRejected[block.ChainType].Enqueue(block);
                    break;
                case ConsensusState.Postponed:
                    break;
                default:
                    break;
            }
        }

        #region Private Functions

        private bool CheckBlockUniqueness(BlockBase block)
        {
            bool result = false;

            return result;
        }

        private void ProcessBlocks(ConcurrentQueue<BlockBase> blocks, ConcurrentQueue<BlockBase> locallyApprovedBlocks, ConcurrentQueue<BlockBase> locallyRejected, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BlockBase blockBase;

                if (blocks.TryDequeue(out blockBase))
                {
                    // TODO: need to understand whether consensus on blocks must be reasched sequentially or it can be done in parallel
                    IChainConsensusService chainConsensysService = _chainConsensusServiceManager.GetChainConsensysService(blockBase.ChainType);

                    chainConsensysService.ReachLocalConsensus(blockBase);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        #endregion Private Functions
    }
}

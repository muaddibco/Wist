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
using Wist.Core.States;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;
using Wist.Node.Core.Model.Blocks;

namespace Wist.Node.Core
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlocksProcessor : IBlocksHandler, IConsumeValidationReport, IRequiresCommunicationHub
    {
        public const string BLOCKS_PROCESSOR_NAME = "ConsensusBlocksProcessor";

        private CancellationToken _cancellationToken;

        private readonly object _sync = new object();
        private readonly INodeContext _nodeContext;
        private readonly IAccountState _accountState;
        private readonly IChainValidationServiceManager _chainConsensusServiceManager;

        private readonly Dictionary<PacketType, ConcurrentQueue<BlockBase>> _blocks;
        private readonly Dictionary<PacketType, ConcurrentQueue<BlockBase>> _locallyApproved;
        private readonly Dictionary<PacketType, ConcurrentQueue<BlockBase>> _consensusAchievedBlocks;
        private readonly IConsensusCheckingService _consensusCheckingService;
        private readonly BlockingCollection<GenericConsensusBlock> _consensusItems; //TODO: need to decide how to know, that decision must be retransmitted

        private ICommunicationService _communicationHub;
        private bool _isInitialized;

        public string Name => BLOCKS_PROCESSOR_NAME;

        public PacketType PacketType => PacketType.Consensus;

        public ConsensusBlocksProcessor(IStatesRepository statesRepository, IChainValidationServiceManager chainConsensusServiceManager, IConsensusCheckingService consensusCheckingService)
        {
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _chainConsensusServiceManager = chainConsensusServiceManager;
            _consensusCheckingService = consensusCheckingService;
            _blocks = new Dictionary<PacketType, ConcurrentQueue<BlockBase>>();
            _locallyApproved = new Dictionary<PacketType, ConcurrentQueue<BlockBase>>();
            _consensusAchievedBlocks = new Dictionary<PacketType, ConcurrentQueue<BlockBase>>();
            _consensusItems = new BlockingCollection<GenericConsensusBlock>();

            foreach (var chainType in Enum.GetValues(typeof(PacketType)))
            {
                _blocks.Add((PacketType)chainType, new ConcurrentQueue<BlockBase>());
                _locallyApproved.Add((PacketType)chainType, new ConcurrentQueue<BlockBase>());
                _consensusAchievedBlocks.Add((PacketType)chainType, new ConcurrentQueue<BlockBase>());
            }
        }

        public void RegisterCommunicationHub(ICommunicationService communicationHub)
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

                foreach (PacketType chainType in Enum.GetValues(typeof(PacketType)))
                {
                    IChainValidationService chainConsensysService = _chainConsensusServiceManager.GetChainValidationService(chainType);
                    chainConsensysService.Initialize(this, ct);

                    Task.Factory.StartNew(o =>
                    {
                        Tuple<PacketType, CancellationToken> inputArgs = (Tuple<PacketType, CancellationToken>)o;

                        ProcessBlocks(_blocks[inputArgs.Item1], inputArgs.Item2);
                    }, new Tuple<PacketType, CancellationToken>( chainType, ct), TaskCreationOptions.LongRunning);
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

            if (_blocks.ContainsKey(blockBase.PacketType))
            {
                _blocks[blockBase.PacketType].Enqueue(blockBase);
            }
        }

        public void OnValidationReport(BlockBase block, IEnumerable<ValidationDecision> consensusDecisions)
        {
            _consensusCheckingService.EnrollConsensusDecisions(block, consensusDecisions);

            if (consensusDecisions.Any(p => p.Participant.PublicKey.Equals32(_accountState.PublicKey)))
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
                    IChainValidationService chainConsensysService = _chainConsensusServiceManager.GetChainValidationService(blockBase.PacketType);

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
                //TODO: accomplish logic for messages delivering
                //_communicationHub.PostMessage(item);
            }
        }

        #endregion Private Functions
    }
}

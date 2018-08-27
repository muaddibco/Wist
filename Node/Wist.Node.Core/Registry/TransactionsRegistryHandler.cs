using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Configuration;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly ITargetBlock<TransactionsShortBlock> _transactionsRegistryConfidenceFlow;
        private readonly ITargetBlock<TransactionsRegistryConfirmationBlock> _confirmationBlockFlow;
        private readonly BlockingCollection<TransactionRegisterBlock> _registrationBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IConfigurationService _configurationService;
        private readonly IRegistryGroupState _registryGroupState;
        private IServerCommunicationService _communicationService;
        private  Timer _timer;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService)
        {
            _registrationBlocks = new BlockingCollection<TransactionRegisterBlock>();
            _registryGroupState = statesRepository.GetInstance<RegistryGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _registryMemPool = registryMemPool;
            _configurationService = configurationService;

            TransformBlock<TransactionsShortBlock, TransactionsRegistryConfidenceBlock> produceConfidenceBlock = new TransformBlock<TransactionsShortBlock, TransactionsRegistryConfidenceBlock>((Func<TransactionsShortBlock, TransactionsRegistryConfidenceBlock>)GetConfidence);
            ActionBlock<TransactionsRegistryConfidenceBlock> sendConfidenceBlock = new ActionBlock<TransactionsRegistryConfidenceBlock>((Action<TransactionsRegistryConfidenceBlock>)SendConfidence);
            produceConfidenceBlock.LinkTo(sendConfidenceBlock);
            _transactionsRegistryConfidenceFlow = produceConfidenceBlock;

            ActionBlock<TransactionsRegistryConfirmationBlock> confirmationProcessingBlock = new ActionBlock<TransactionsRegistryConfirmationBlock>((Action<TransactionsRegistryConfirmationBlock>)ProcessConfirmationBlock);
            _confirmationBlockFlow = confirmationProcessingBlock;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _communicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().UdpServiceName);

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            TransactionRegisterBlock transactionRegisterBlock = blockBase as TransactionRegisterBlock;

            if(transactionRegisterBlock != null)
            {
                _registrationBlocks.Add(transactionRegisterBlock);
            }

            TransactionsShortBlock transactionsShortBlock = blockBase as TransactionsShortBlock;
            if(transactionsShortBlock != null)
            {
                _transactionsRegistryConfidenceFlow.Post(transactionsShortBlock);
            }

            TransactionsRegistryConfirmationBlock confirmationBlock = blockBase as TransactionsRegistryConfirmationBlock;
            if(confirmationBlock != null && ValidateConfirmationBlock(confirmationBlock))
            {
                _confirmationBlockFlow.Post(confirmationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (TransactionRegisterBlock transactionRegisterBlock in _registrationBlocks.GetConsumingEnumerable(ct))
            {
                if(_timer == null)
                {
                    _timer = new Timer(new TimerCallback(TimerElapsed), _registryMemPool, 120000, Timeout.Infinite);
                }

                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

                bool isNew = _registryMemPool.EnqueueTransactionRegisterBlock(transactionRegisterBlock);

                if (isNew)
                {
                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterBlock);
                    _communicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        private void TimerElapsed(object o)
        {

        }

        private TransactionsRegistryConfidenceBlock GetConfidence(TransactionsShortBlock transactionsShortBlock)
        {
            TransactionsRegistryConfidenceBlock transactionsRegistryConfidenceBlock = new TransactionsRegistryConfidenceBlock();

            return transactionsRegistryConfidenceBlock;
        }

        private void SendConfidence(TransactionsRegistryConfidenceBlock transactionsRegistryConfidenceBlock)
        {

        }

        private bool ValidateConfirmationBlock(TransactionsRegistryConfirmationBlock confirmationBlock)
        {
            return true;
        }

        private void ProcessConfirmationBlock(TransactionsRegistryConfirmationBlock confirmationBlock)
        {
            _registryGroupState.ToggleLastBlockConfirmationReceived();

            //TODO: obtain Transactions Registry Short block from MemPool by hash given in confirmationBlock
            //TODO: clear MemPool from Transaction Headers of confirmed Short Block
        }

        #endregion PrivateFunctions
    }
}

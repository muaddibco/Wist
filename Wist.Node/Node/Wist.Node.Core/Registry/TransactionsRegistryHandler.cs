using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers.RawPackets;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Configuration;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Node.Core.Common;
using Wist.BlockLattice.Core.Serializers;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly ITargetBlock<RegistryShortBlock> _transactionsRegistryConfidenceFlow;
        private readonly ITargetBlock<RegistryConfirmationBlock> _confirmationBlockFlow;
        private readonly BlockingCollection<RegistryRegisterBlock> _registrationBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IConfigurationService _configurationService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly IRegistryGroupState _registryGroupState;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _defaulHashCalculation;
        private readonly IHashCalculation _powCalculation;
        private readonly INodeContext _nodeContext;
        private IServerCommunicationService _udpCommunicationService;
        private IServerCommunicationService _tcpCommunicationService;
        private  Timer _timer;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, 
            IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, 
            IHashCalculationsRepository hashCalculationRepository, ISignatureSupportSerializersFactory signatureSupportSerializersFactory)
        {
            _registrationBlocks = new BlockingCollection<RegistryRegisterBlock>();
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _registryMemPool = registryMemPool;
            _configurationService = configurationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _defaulHashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            _powCalculation = hashCalculationRepository.Create(Globals.POW_TYPE);

            TransformBlock<RegistryShortBlock, RegistryConfidenceBlock> produceConfidenceBlock = new TransformBlock<RegistryShortBlock, RegistryConfidenceBlock>((Func<RegistryShortBlock, RegistryConfidenceBlock>)GetConfidence);
            ActionBlock<RegistryConfidenceBlock> sendConfidenceBlock = new ActionBlock<RegistryConfidenceBlock>((Action<RegistryConfidenceBlock>)SendConfidence);
            produceConfidenceBlock.LinkTo(sendConfidenceBlock);
            _transactionsRegistryConfidenceFlow = produceConfidenceBlock;

            ActionBlock<RegistryConfirmationBlock> confirmationProcessingBlock = new ActionBlock<RegistryConfirmationBlock>((Action<RegistryConfirmationBlock>)ProcessConfirmationBlock);
            _confirmationBlockFlow = confirmationProcessingBlock;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _udpCommunicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().UdpServiceName);
            _tcpCommunicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().TcpServiceName);

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ProcessBlock(BlockBase blockBase)
        {

            if (blockBase is RegistryRegisterBlock transactionRegisterBlock)
            {
                _registrationBlocks.Add(transactionRegisterBlock);
            }

            if (blockBase is RegistryShortBlock transactionsShortBlock)
            {
                _transactionsRegistryConfidenceFlow.Post(transactionsShortBlock);
            }

            if (blockBase is RegistryConfirmationBlock confirmationBlock && ValidateConfirmationBlock(confirmationBlock))
            {
                _confirmationBlockFlow.Post(confirmationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryRegisterBlock transactionRegisterBlock in _registrationBlocks.GetConsumingEnumerable(ct))
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
                    _udpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        private void TimerElapsed(object o)
        {

        }

        private RegistryConfidenceBlock GetConfidence(RegistryShortBlock transactionsShortBlock)
        {
            byte[] proof = _registryMemPool.GetConfidenceMask(transactionsShortBlock, out byte[] bitMask);

            RegistryConfidenceBlock transactionsRegistryConfidenceBlock = new RegistryConfidenceBlock()
            {
                SyncBlockHeight = transactionsShortBlock.SyncBlockHeight,
                PowHash = _powCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]),
                BlockHeight = transactionsShortBlock.BlockHeight,
                ReferencedBlockHash = _defaulHashCalculation.CalculateHash(transactionsShortBlock.BodyBytes),
                BitMask = bitMask,
                ConfidenceProof = proof
            };

            return transactionsRegistryConfidenceBlock;
        }

        private void SendConfidence(RegistryConfidenceBlock transactionsRegistryConfidenceBlock)
        {
            ISignatureSupportSerializer confidenceBlockSerializer = _signatureSupportSerializersFactory.Create(transactionsRegistryConfidenceBlock);
            _tcpCommunicationService.PostMessage(_registryGroupState.SyncLayerNode, confidenceBlockSerializer);
        }

        private bool ValidateConfirmationBlock(RegistryConfirmationBlock confirmationBlock)
        {
            return true;
        }

        private void ProcessConfirmationBlock(RegistryConfirmationBlock confirmationBlock)
        {
            _registryGroupState.ToggleLastBlockConfirmationReceived();

            

            //TODO: obtain Transactions Registry Short block from MemPool by hash given in confirmationBlock
            //TODO: clear MemPool from Transaction Headers of confirmed Short Block
        }

        #endregion PrivateFunctions
    }
}

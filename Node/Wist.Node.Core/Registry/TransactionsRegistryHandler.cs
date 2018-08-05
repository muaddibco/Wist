using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Communication;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Registry
{
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly BlockingCollection<TransactionRegisterBlock> _registrationBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly INeighborhoodState _neighborhoodState;
        private readonly IMemPool<TransactionRegisterBlock> _transactionRegisterBlocksMemPool;
        private IServerCommunicationService _communicationService;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IMemPoolsRepository memPoolsRepository)
        {
            _neighborhoodState = statesRepository.GetInstance<RegistryGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _transactionRegisterBlocksMemPool = memPoolsRepository.GetInstance<TransactionRegisterBlock>();
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GenericUdp");

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
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (TransactionRegisterBlock transactionRegisterBlock in _registrationBlocks.GetConsumingEnumerable(ct))
            {
                bool isNew = _transactionRegisterBlocksMemPool.AddIfNotExist(transactionRegisterBlock);

                if (isNew)
                {
                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterBlock);
                    _communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        #endregion PrivateFunctions
    }
}

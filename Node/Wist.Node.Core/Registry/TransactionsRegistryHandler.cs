using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly BlockingCollection<TransactionRegisterBlock> _registrationBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly INeighborhoodState _neighborhoodState;
        private IServerCommunicationService _communicationService;
        private  Timer _timer;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool)
        {
            _registrationBlocks = new BlockingCollection<TransactionRegisterBlock>();
            _neighborhoodState = statesRepository.GetInstance<RegistryGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _registryMemPool = registryMemPool;
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
                if(_timer == null)
                {
                    _timer = new Timer(new TimerCallback(TimerElapsed), _registryMemPool, 120000, Timeout.Infinite);
                }

                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

                bool isNew = _registryMemPool.EnqueueTransactionRegisterBlock(transactionRegisterBlock);

                if (isNew)
                {
                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterBlock);
                    _communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        private void TimerElapsed(object o)
        {

        }

        #endregion PrivateFunctions
    }
}

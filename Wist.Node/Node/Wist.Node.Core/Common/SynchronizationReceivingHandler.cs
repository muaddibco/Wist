using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers.RawPackets;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Cryptography;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Core.HashCalculations;
using Wist.Blockchain.Core;
using Wist.Core.Models;
using Wist.Core;

namespace Wist.Node.Core.Common
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationReceivingHandler : IBlocksHandler
    {
        public const string NAME = "SynchronizationReceiving";
        private readonly BlockingCollection<SynchronizationConfirmedBlock> _synchronizationBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly INeighborhoodState _neighborhoodState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IChainDataService _chainDataService;
        private readonly IHashCalculation _hashCalculation;
        private IServerCommunicationService _communicationService;
        private uint _lastRetransmittedSyncBlockHeight;

        public SynchronizationReceivingHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IChainDataServicesManager chainDataServicesManager, IHashCalculationsRepository hashCalculationsRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _synchronizationBlocks = new BlockingCollection<SynchronizationConfirmedBlock>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            //TODO: need to move definition of communication service name to configuration file
            _communicationService = _communicationServicesRegistry.GetInstance("GenericUdp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(PacketBase blockBase)
        {
            SynchronizationConfirmedBlock synchronizationBlock = blockBase as SynchronizationConfirmedBlock;

            if(synchronizationBlock != null)
            {
                _synchronizationBlocks.Add(synchronizationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (SynchronizationConfirmedBlock synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                if ((_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0) >= synchronizationBlock.BlockHeight)
                {
                    continue;
                }

                _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationBlock.BlockHeight, _hashCalculation.CalculateHash(synchronizationBlock.RawData), synchronizationBlock.ReportedTime, DateTime.Now, synchronizationBlock.Round));

                _chainDataService.Add(synchronizationBlock);

                IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(synchronizationBlock);
                _communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), packetProvider);
            }
        }

        #endregion Private Functions
    }
}

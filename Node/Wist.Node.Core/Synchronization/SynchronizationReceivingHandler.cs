using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationReceivingHandler : IBlocksHandler
    {
        public const string NAME = "SynchronizationReceiving";
        private readonly BlockingCollection<SynchronizationProducingBlock> _synchronizationBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly INeighborhoodState _neighborhoodState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private IServerCommunicationService _communicationService;
        private uint _lastRetransmittedSyncBlockHeight;

        public SynchronizationReceivingHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _synchronizationBlocks = new BlockingCollection<SynchronizationProducingBlock>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GenericUdp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            SynchronizationProducingBlock synchronizationBlock = blockBase as SynchronizationProducingBlock;

            if(synchronizationBlock != null)
            {
                _synchronizationBlocks.Add(synchronizationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            List<SynchronizationBlockBase> synchronizationBlocksPerLoop = new List<SynchronizationBlockBase>();

            foreach (SynchronizationProducingBlock synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                if (_synchronizationContext.LastBlockDescriptor.BlockHeight >= synchronizationBlock.BlockHeight)
                {
                    continue;
                }

                _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationBlock.BlockHeight, synchronizationBlock.Hash, synchronizationBlock.ReportedTime, DateTime.Now));
                IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(synchronizationBlock);
                _communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), packetProvider);
            }
        }

        #endregion Private Functions
    }
}

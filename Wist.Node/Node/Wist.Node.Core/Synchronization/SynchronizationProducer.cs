using System;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Common;
using Wist.BlockLattice.Core;
using Wist.Core.HashCalculations;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationProducer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducer : ISynchronizationProducer
    {
        private readonly ISerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private readonly IHashCalculation _proofOfWorkCalculation;
        private IServerCommunicationService _communicationService;
        private ulong _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SynchronizationProducer(ISerializersFactory signatureSupportSerializersFactory, IStatesRepository statesRepository, 
            IServerCommunicationServicesRegistry communicationServicesRegistry, IConfigurationService configurationService, 
            IHashCalculationsRepository hashCalculationsRepository)
        {
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _proofOfWorkCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
        }

        public void Initialize()
        {
            _communicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<ISynchronizationConfiguration>().CommunicationServiceName);
        }

        public void DeferredBroadcast(ushort round, Action onBroadcasted)
        {
            if (_synchronizationContext == null)
            {
                return;
            }

            //if (_synchronizationContext.LastBlockDescriptor != null && _synchronizationContext.LastBlockDescriptor.BlockHeight - 1 > _lastLaunchedSyncBlockOrder)
            //{
            //    _syncProducingCancellation?.Cancel();
            //    _syncProducingCancellation = null;
            //}

            _syncProducingCancellation = new CancellationTokenSource();

            int delay = Math.Max((int)(60000 * round - (DateTime.Now - (_synchronizationContext.LastBlockDescriptor?.UpdateTime ?? DateTime.Now)).TotalMilliseconds), 0);

            if (delay > 0)
            {
                Task.Delay(delay, _syncProducingCancellation.Token)
                    .ContinueWith(t =>
                    {
                        try
                        {
                            SynchronizationDescriptor synchronizationDescriptor = _synchronizationContext.LastBlockDescriptor;

                            SynchronizationProducingBlock synchronizationBlock = new SynchronizationProducingBlock
                            {
                                SyncBlockHeight = synchronizationDescriptor?.BlockHeight ?? 0,
                                BlockHeight = (synchronizationDescriptor?.BlockHeight ?? 0) + 1,
                                ReportedTime = synchronizationDescriptor?.MedianTime.AddMinutes(1) ?? DateTime.Now,
                                Round = round,
                                HashPrev = _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE],
                                PowHash = _proofOfWorkCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE])
                            };

                            using (ISerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(synchronizationBlock))
                            {
                                _communicationService.PostMessage(_synchronizationGroupState.GetAllNeighbors(), signatureSupportSerializer);
                                _lastLaunchedSyncBlockOrder = synchronizationBlock.BlockHeight;
                            }
                        }
                        finally
                        {
                            onBroadcasted.Invoke();
                        }

                    }, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
            }
        }

        public void CancelDeferredBroadcast()
        {
            _syncProducingCancellation?.Cancel();
        }
    }
}

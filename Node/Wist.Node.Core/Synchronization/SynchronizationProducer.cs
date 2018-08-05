using System;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationProducer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducer : ISynchronizationProducer
    {
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private IServerCommunicationService _communicationService;
        private ulong _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SynchronizationProducer(ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IConfigurationService configurationService)
        {
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
        }

        public void Initialize()
        {
            
            _communicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<SynchronizationConfiguration>().CommunicationServiceName);
        }

        public void DeferredBroadcast()
        {
            if (_synchronizationContext == null)
            {
                return;
            }

            if(_synchronizationContext.LastBlockDescriptor == null || _synchronizationContext.LastBlockDescriptor.BlockHeight > _lastLaunchedSyncBlockOrder)
            {
                if(_synchronizationContext.LastBlockDescriptor != null &&_synchronizationContext.LastBlockDescriptor.BlockHeight - 1 > _lastLaunchedSyncBlockOrder)
                {
                    _syncProducingCancellation?.Cancel();
                    _syncProducingCancellation = null;
                }
                else
                {
                    _syncProducingCancellation = new CancellationTokenSource();
                }

                int delay = (int)(60000 - (DateTime.Now - (_synchronizationContext.LastBlockDescriptor?.UpdateTime ?? DateTime.Now)).TotalMilliseconds);

                if (delay > 0)
                {
                    Task.Delay(delay, _syncProducingCancellation.Token)
                        .ContinueWith((t, o) =>
                        {
                            SynchronizationDescriptor synchronizationDescriptor = o as SynchronizationDescriptor;

                            SynchronizationProducingBlock synchronizationBlock = new SynchronizationProducingBlock
                            {
                                BlockHeight = synchronizationDescriptor?.BlockHeight ?? 0 + 1,
                                ReportedTime = synchronizationDescriptor?.MedianTime.AddMinutes(1) ?? DateTime.Now
                            };

                            using (ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(synchronizationBlock))
                            {
                                _communicationService.PostMessage(_synchronizationGroupState.GetAllParticipants(), signatureSupportSerializer);
                                _lastLaunchedSyncBlockOrder = synchronizationBlock.BlockHeight;
                            }

                        }, _synchronizationContext.LastBlockDescriptor, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
                }
            }
        }
    }
}

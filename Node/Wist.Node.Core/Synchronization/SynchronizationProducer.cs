using System;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
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
        private readonly ICommunicationServicesRegistry _communicationServicesRegistry;
        private ICommunicationService _communicationService;
        private uint _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SynchronizationProducer(ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IStatesRepository statesRepository, ICommunicationServicesRegistry communicationServicesRegistry)
        {
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
        }

        public void Initialize()
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GenericTcp");
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

                int delay = (int)(60000 - (DateTime.Now - (_synchronizationContext.LastBlockDescriptor?.ReceivingTime ?? DateTime.Now)).TotalMilliseconds);

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

                            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncProducingBlock);
                            byte[] body = signatureSupportSerializer.GetBody(synchronizationBlock);
                            byte[] signature = _nodeContext.Sign(body);
                            synchronizationBlock.PublicKey = _nodeContext.PublicKey;
                            synchronizationBlock.Signature = signature;

                            //TODO: accomplish logic for messages delivering
                            //_communicationHub.PostMessage(synchronizationBlock);
                            _lastLaunchedSyncBlockOrder = synchronizationBlock.BlockHeight;
                        }, _synchronizationContext.LastBlockDescriptor, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
                }
            }
        }
    }
}

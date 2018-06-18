using System;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationProducer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducer : ISynchronizationProducer
    {
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly ICommunicationServicesRegistry _communicationServicesRegistry;
        private ICommunicationService _communicationService;
        private uint _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SynchronizationProducer(ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodeContext nodeContext, ICommunicationServicesRegistry communicationServicesRegistry)
        {
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = nodeContext;
            _communicationServicesRegistry = communicationServicesRegistry;
        }

        public void Initialize()
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GeneralTcp");
        }

        public void DeferredBroadcast()
        {
            if (_nodeContext.SynchronizationContext  == null)
            {
                return;
            }

            if(_nodeContext.SynchronizationContext.LastBlockDescriptor.BlockHeight > _lastLaunchedSyncBlockOrder)
            {
                if(_nodeContext.SynchronizationContext.LastBlockDescriptor.BlockHeight - 1 > _lastLaunchedSyncBlockOrder)
                {
                    _syncProducingCancellation?.Cancel();
                }

                int delay = (int)(60000 - (DateTime.Now - _nodeContext.SynchronizationContext.LastBlockDescriptor.ReceivingTime).TotalMilliseconds);

                if (delay > 0)
                {
                    Task.Delay(delay, _syncProducingCancellation.Token)
                        .ContinueWith((t, o) =>
                        {
                            SynchronizationDescriptor synchronizationDescriptor = o as SynchronizationDescriptor;

                            SynchronizationBlock synchronizationBlock = new SynchronizationBlock
                            {
                                BlockHeight = synchronizationDescriptor.BlockHeight + 1,
                                ReportedTime = synchronizationDescriptor.MedianTime.AddMinutes(1)
                            };

                            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncBlock);
                            byte[] body = signatureSupportSerializer.GetBody(synchronizationBlock);
                            byte[] signature = _nodeContext.Sign(body);
                            synchronizationBlock.PublicKey = _nodeContext.PublicKey;
                            synchronizationBlock.Signature = signature;

                            //TODO: accomplish logic for messages delivering
                            //_communicationHub.PostMessage(synchronizationBlock);
                            _lastLaunchedSyncBlockOrder = synchronizationBlock.BlockHeight;
                        }, _nodeContext.SynchronizationContext.LastBlockDescriptor, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
                }
            }
        }
    }
}

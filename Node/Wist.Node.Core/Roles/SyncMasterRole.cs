using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core.Roles
{
    /// <summary>
    /// <see cref="SyncMasterRole"/> is responsible for producing Synchronization Packets every 60 second (considering when last synchronization packet was obtained) and distribute created Synchronization Block to all network participants
    /// </summary>
    public class SyncMasterRole : RoleBase
    {
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly INodeContext _nodeContext;
        private readonly ICommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ITargetBlock<string> _syncContextUpdated;
        private ICommunicationService _communicationService;
        private uint _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SyncMasterRole(IStatesRepository statesRepository, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, ICommunicationServicesRegistry communicationServicesRegistry)
        {
            _synchronizationContext = statesRepository.GetInstance<Wist.Core.Synchronization.SynchronizationContext>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;

            _syncContextUpdated = new ActionBlock<string>(s =>
             {
                 DeferredBroadcast();
             });
        }

        protected override void InitializeInner()
        {
            _synchronizationContext.SubscribeOnStateChange(_syncContextUpdated);
            _communicationService = _communicationServicesRegistry.GetInstance("GeneralTcp");
        }

        public override void Start()
        {
            DeferredBroadcast();
        }

        public override void Stop()
        {
            _syncProducingCancellation?.Cancel();
        }

        private void DeferredBroadcast()
        {
            if (_synchronizationContext == null)
            {
                return;
            }

            if (_synchronizationContext.LastBlockDescriptor.BlockHeight > _lastLaunchedSyncBlockOrder)
            {
                if (_synchronizationContext.LastBlockDescriptor.BlockHeight - 1 > _lastLaunchedSyncBlockOrder)
                {
                    _syncProducingCancellation?.Cancel();
                }

                int delay = (int)(60000 - (DateTime.Now - _synchronizationContext.LastBlockDescriptor.ReceivingTime).TotalMilliseconds);

                if (delay > 0)
                {
                    Task.Delay(delay, _syncProducingCancellation.Token)
                        .ContinueWith((t, o) =>
                        {
                            SynchronizationDescriptor synchronizationDescriptor = o as SynchronizationDescriptor;

                            SynchronizationProducingBlock synchronizationBlock = new SynchronizationProducingBlock
                            {
                                BlockHeight = synchronizationDescriptor.BlockHeight + 1,
                                ReportedTime = synchronizationDescriptor.MedianTime.AddMinutes(1)
                            };

                            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(PacketType.Synchronization, BlockTypes.Synchronization_TimeSyncProducingBlock);

                            try
                            {
                                _communicationService.PostMessage(_synchronizationGroupState.GetAllParticipants(), signatureSupportSerializer);
                                _lastLaunchedSyncBlockOrder = synchronizationBlock.BlockHeight;
                            }
                            finally
                            {
                                if(signatureSupportSerializer != null)
                                {
                                    _signatureSupportSerializersFactory.Utilize(signatureSupportSerializer);
                                }
                            }
                        }, _synchronizationContext.LastBlockDescriptor, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
                }
            }
        }
    }
}

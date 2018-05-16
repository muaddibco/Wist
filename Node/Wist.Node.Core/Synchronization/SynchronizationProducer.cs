﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationProducer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducer : ISynchronizationProducer
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private ICommunicationHub _communicationHub;
        private uint _lastLaunchedSyncBlockOrder;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SynchronizationProducer(ISynchronizationContext synchronizationContext, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodeContext nodeContext)
        {
            _synchronizationContext = synchronizationContext;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = nodeContext;
        }

        public void Initialize(ICommunicationHub communicationHub)
        {
            _communicationHub = communicationHub;
        }

        public void Launch()
        {
            if (_synchronizationContext == null)
            {
                return;
            }

            if(_synchronizationContext.LastSyncBlock.BlockOrder > _lastLaunchedSyncBlockOrder)
            {
                if(_synchronizationContext.LastSyncBlock.BlockOrder -1 > _lastLaunchedSyncBlockOrder)
                {
                    _syncProducingCancellation?.Cancel();
                }

                int delay = (int)(60000 - (DateTime.Now - _synchronizationContext.LastSyncBlockReceivingTime).TotalMilliseconds);

                if (delay > 0)
                {
                    Task.Delay(delay, _syncProducingCancellation.Token)
                        .ContinueWith((t, o) =>
                        {
                            SynchronizationConfirmedBlock confirmedBlock = o as SynchronizationConfirmedBlock;

                            SynchronizationBlockV1 synchronizationBlock = new SynchronizationBlockV1
                            {
                                BlockOrder = confirmedBlock.BlockOrder + 1,
                                ReportedTime = confirmedBlock.ReportedTime.AddMinutes(1)
                            };

                            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(ChainType.Synchronization, BlockTypes.Synchronization_TimeSyncBlock);
                            byte[] body = signatureSupportSerializer.GetBody(synchronizationBlock);
                            byte[] signature = _nodeContext.Sign(body);
                            synchronizationBlock.PublicKey = _nodeContext.PublicKey;
                            synchronizationBlock.Signature = signature;

                            _communicationHub.BroadcastMessage(synchronizationBlock);
                        }, _synchronizationContext.LastSyncBlock, _syncProducingCancellation.Token, TaskContinuationOptions.NotOnCanceled, TaskScheduler.Current);
                }
            }
        }
    }
}

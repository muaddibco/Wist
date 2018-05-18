using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Node.Core.Configuration;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationService : ISynchronizationService
    {
        public const int GROUP_PARTICIPATION_CHECK_PERIOD = 60;

        private readonly object _joinSync = new object();
        private readonly ILog _log = LogManager.GetLogger(typeof(SynchronizationService));
        private ICommunicationHub _communicationHubSync;
        private readonly IConfigurationService _configurationService;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _groupParticipationCheckingCancellationToken;

        private bool _joinedToSyncGroup;

        public SynchronizationService(ICommunicationHubFactory communicationHubFactory, IConfigurationService configurationService, ISynchronizationContext synchronizationContext, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodeContext nodeContext, ISynchronizationProducer synchronizationProducer)
        {
            _communicationHubSync = communicationHubFactory.Create();
            _configurationService = configurationService;
            _synchronizationContext = synchronizationContext;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = nodeContext;
            _synchronizationProducer = synchronizationProducer;
        }

        /// <summary>
        /// Function does following:
        ///   1. Starts to listen on port for Synchronization Blocks receiving. All other operations of entire Node must be suspended until last valid Synchronization Block is obtained
        ///   2. Starts process of checking whether current node must participate in Synchronization Group
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Initialize(IBlocksProcessor blocksProcessor, CancellationToken cancellationToken)
        {
               SynchronizationCommunicationConfiguration syncCommunicationConfiguration = (SynchronizationCommunicationConfiguration)_configurationService[SynchronizationCommunicationConfiguration.SECTION_NAME];
            _communicationHubSync.Init(
                new SocketListenerSettings(
                    syncCommunicationConfiguration.MaxConnections, // TODO: this value must be taken from the corresponding chain from block-lattice
                    syncCommunicationConfiguration.MaxPendingConnections,
                    syncCommunicationConfiguration.MaxSimultaneousAcceptOps,
                    syncCommunicationConfiguration.ReceiveBufferSize, 2,
                    new IPEndPoint(IPAddress.Loopback, syncCommunicationConfiguration.ListeningPort), false), blocksProcessor);

            _communicationHubSync.StartListen();

            _synchronizationProducer.Initialize(_communicationHubSync);

            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => 
            {
                _groupParticipationCheckingCancellationToken?.Cancel();
                _groupParticipationCheckingCancellationToken = null;
            });
        }

        public void Start()
        {
            StartCheckForGroupParticipation();
        }

        private void StartCheckForGroupParticipation()
        {
            _groupParticipationCheckingCancellationToken?.Cancel();
            _groupParticipationCheckingCancellationToken = new CancellationTokenSource();

            PeriodicTaskFactory.Start(() => 
            {
                if (!_cancellationToken.IsCancellationRequested)
                {
                    bool shouldJoin = CheckShouldJoinSyncGroup();

                    if (shouldJoin)
                    {
                        if (JoinSyncGroup())
                        {
                            InitializeSynchronizationWorker();
                        }
                    }
                    else
                    {
                        if (LeaveSyncGroup())
                        {
                        }
                    }
                }
            }, GROUP_PARTICIPATION_CHECK_PERIOD, cancelToken: _cancellationToken);
        }

        

        private bool CheckShouldJoinSyncGroup()
        {
            // TODO: replace with actual check
            return true;
        }

        private bool CheckShouldLeaveSyncGroup()
        {
            // TODO: replace with actual check
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool JoinSyncGroup()
        {
            if (_joinedToSyncGroup)
                return false;

            lock(_joinSync)
            {
                if (_joinedToSyncGroup)
                    return false;

                try
                {
                    // TODO: replace with actual logic

                    _joinedToSyncGroup = true;

                    return true;
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to join Synchronization Group", ex);
                    return false;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LeaveSyncGroup()
        {
            if (!_joinedToSyncGroup)
                return false;

            lock (_joinSync)
            {
                if (!_joinedToSyncGroup)
                    return false;

                try
                {
                    // TODO: replace with actual logic

                    _joinedToSyncGroup = false;

                    return true;
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to leave Synchronization Group", ex);
                    return false;
                }
            }
        }

        private void InitializeSynchronizationWorker()
        {
            if (_cancellationToken.IsCancellationRequested)
                return;

            Task.Run(() =>
            {
                while (_synchronizationContext.LastSyncBlock == null)
                {
                    Thread.Sleep(60000);
                }

                DistributeReadyForParticipationMessage();

                _synchronizationProducer.DeferredBroadcast();
            });
        }

        private void DistributeReadyForParticipationMessage()
        {
            ISignatureSupportSerializer serializer = _signatureSupportSerializersFactory.Create(ChainType.Synchronization, BlockTypes.Synchronization_ReadyToParticipateBlock);
            ReadyForParticipationBlock block = new ReadyForParticipationBlock() { BlockOrder = _synchronizationContext.LastSyncBlock.BlockOrder };
            byte[] body = serializer.GetBody(block);
            byte[] signature = _nodeContext.Sign(body);
            block.PublicKey = _nodeContext.PublicKey;
            block.Signature = signature;
            _communicationHubSync.BroadcastMessage(block);
        }
    }
}

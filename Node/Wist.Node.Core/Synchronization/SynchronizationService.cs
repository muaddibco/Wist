using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Configuration;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationService : ISynchronizationService
    {
        public const int GROUP_PARTICIPATION_CHECK_PERIOD = 60;

        private readonly object _joinSync = new object();
        private readonly ILogger _log;
        private ICommunicationService _communicationHubSync;
        private readonly IConfigurationService _configurationService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly IAccountState _accountState;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly IDposService _dposService;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _groupParticipationCheckingCancellationToken;

        private bool _joinedToSyncGroup;

        public SynchronizationService(ICommunicationServicesRepository communicationHubFactory, IConfigurationService configurationService, 
            ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IStatesRepository statesRepository, 
            ISynchronizationProducer synchronizationProducer, IDposService dposService, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            //_communicationHubSync = communicationHubFactory.Create();
            _configurationService = configurationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationProducer = synchronizationProducer;
            _dposService = dposService;
        }

        /// <summary>
        /// Function does following:
        ///   1. Starts to listen on port for Synchronization Blocks receiving. All other operations of entire Node must be suspended until last valid Synchronization Block is obtained
        ///   2. Starts process of checking whether current node must participate in Synchronization Group
        /// </summary>
        /// <param name="cancellationToken"></param>
        public void Initialize(CancellationToken cancellationToken)
        {
            _synchronizationProducer.Initialize();

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
            SortedDictionary<ushort, byte[]> topParticipants = _dposService.GetTopNodesPublicKeys(_nodeContext.SyncGroupParticipantsCount);

            bool result = topParticipants.Any(t => t.Value.Equals32(_accountState.PublicKey));

            return result;
        }

        private bool CheckShouldLeaveSyncGroup()
        {
            SortedDictionary<ushort, byte[]> topParticipants = _dposService.GetTopNodesPublicKeys(_nodeContext.SyncGroupParticipantsCount);

            bool result = topParticipants.Any(t => t.Value.Equals32(_accountState.PublicKey));

            return !result;
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
                    //TODO: replace with actual logic

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
                    //TODO: replace with actual logic

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
                while (_synchronizationContext.LastBlockDescriptor == null)
                {
                    Thread.Sleep(60000);
                }

                DistributeReadyForParticipationMessage();

                _synchronizationProducer.DeferredBroadcast();
            });
        }

        private void DistributeReadyForParticipationMessage()
        {
            ReadyForParticipationBlock block = new ReadyForParticipationBlock() { BlockHeight = _synchronizationContext.LastBlockDescriptor.BlockHeight };
            ISignatureSupportSerializer serializer = _signatureSupportSerializersFactory.Create(block);

            try
            {
                //TODO: accomplish logic for messages distribution
                //_communicationHubSync.PostMessage(block);
            }
            finally
            {
                _signatureSupportSerializersFactory.Utilize(serializer);
            }
        }
    }
}

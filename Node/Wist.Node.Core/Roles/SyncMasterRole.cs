using System.Threading;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core.Roles
{

    [RegisterExtension(typeof(IRole), Lifetime = LifetimeManagement.Singleton)]
    /// <summary>
    /// <see cref="SyncMasterRole"/> is responsible for producing Synchronization Packets every 60 second (considering when last synchronization packet was obtained) and distribute created Synchronization Block to all network participants
    /// </summary>
    public class SyncMasterRole : RoleBase
    {
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly INodeContext _nodeContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISynchronizationGroupParticipationService _synchronizationGroupParticipationService;
        private IServerCommunicationService _communicationService;
        private uint _lastLaunchedSyncBlockOrder = 0;
        private CancellationTokenSource _syncProducingCancellation = null;

        public SyncMasterRole(IStatesRepository statesRepository, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IServerCommunicationServicesRegistry communicationServicesRegistry, ISynchronizationGroupParticipationService synchronizationGroupParticipationService)
        {
            _nodeContext = statesRepository.GetInstance<NodeContext>();
            _synchronizationGroupState = statesRepository.GetInstance<SynchronizationGroupState>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _synchronizationGroupParticipationService = synchronizationGroupParticipationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
        }

        protected override void InitializeInner()
        {
            _communicationService = _communicationServicesRegistry.GetInstance("GeneralTcp");
            _synchronizationGroupParticipationService.Initialize();
        }

        public override void Start()
        {
            _synchronizationGroupParticipationService.Start();
        }

        public override void Stop()
        {
            _syncProducingCancellation?.Cancel();
        }
    }
}

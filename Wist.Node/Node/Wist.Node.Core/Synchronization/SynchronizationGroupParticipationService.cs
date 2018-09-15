using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.DPOS;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationGroupParticipationService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupParticipationService : ISynchronizationGroupParticipationService
    {
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly INodeDposProvider _nodeDposProvider;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IAccountState _accountState;
        private readonly TransformBlock<string, string> _synchronizationGroupParticipationCheckAction;
        private readonly ActionBlock<string> _synchronizationGroupLeaderCheckAction;
        private readonly object _sync = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isParticipating;
        private bool _isStarted;
        private IDisposable _synchronozationGroupLeaderCheckUnsubscriber;

        public SynchronizationGroupParticipationService(ISynchronizationProducer synchronizationProducer, IStatesRepository statesRepository, INodeDposProvidersFactory nodeDposProvidersFactory)
        {
            _synchronizationProducer = synchronizationProducer;
            _nodeDposProvider = nodeDposProvidersFactory.Create(PacketType.TransactionalChain);
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationGroupParticipationCheckAction = new TransformBlock<string, string>((Func<string, string>)SynchronizationGroupParticipationCheckAction);
            _synchronizationGroupLeaderCheckAction = new ActionBlock<string>(SynchronizationGroupLeaderCheckAction);
        }

        public void Initialize()
        {
            
        }

        public void Start()
        {
            if(_isStarted)
            {
                return;
            }

            lock(_sync)
            {
                if(_isStarted)
                {
                    return;
                }

                _synchronizationContext.SubscribeOnStateChange(_synchronizationGroupParticipationCheckAction);

                _isStarted = true;

                _synchronizationGroupParticipationCheckAction.Post(null);
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        #region Private Functions

        private void CheckSynchronizationGroupParticipation()
        {
            //TODO: add real check for participation
            int rating = _nodeDposProvider.GetCandidateRating(_accountState.AccountKey);
        }

        private string SynchronizationGroupParticipationCheckAction(string arg)
        {
            if (!_isParticipating)
            {
                _isParticipating = true;
                _synchronizationProducer.Initialize();

                _synchronozationGroupLeaderCheckUnsubscriber = _synchronizationGroupParticipationCheckAction.LinkTo(_synchronizationGroupLeaderCheckAction);
                _synchronizationGroupLeaderCheckAction.Post(null);
            }

            return arg;
        }

        private async Task SynchronizationGroupLeaderCheckAction(string arg)
        {
            _synchronizationProducer.DeferredBroadcast();
        }

        #endregion Private Functions
    }

}

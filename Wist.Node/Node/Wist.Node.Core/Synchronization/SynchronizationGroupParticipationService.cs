using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Rating;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationGroupParticipationService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupParticipationService : ISynchronizationGroupParticipationService
    {
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly INodesRatingProvider _nodesRatingProvider;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly IAccountState _accountState;
        private readonly TransformBlock<string, string> _synchronizationGroupParticipationCheckAction;
        private readonly ActionBlock<string> _synchronizationGroupLeaderCheckAction;
        private readonly object _sync = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isParticipating;
        private bool _isStarted;
        private bool _round;
        private int _ratingPosition;
        private IDisposable _synchronozationGroupLeaderCheckUnsubscriber;

        public SynchronizationGroupParticipationService(ISynchronizationProducer synchronizationProducer, IStatesRepository statesRepository, INodesRatingProviderFactory nodesRatingProvidersFactory)
        {
            _synchronizationProducer = synchronizationProducer;
            _nodesRatingProvider = nodesRatingProvidersFactory.Create(PacketType.TransactionalChain);
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _synchronizationGroupParticipationCheckAction = new TransformBlock<string, string>((Func<string, string>)SynchronizationGroupParticipationCheckAction);
            _synchronizationGroupLeaderCheckAction = new ActionBlock<string>((Action<string>)SynchronizationGroupLeaderCheckAction);
        }

        public void Initialize()
        {
            _nodesRatingProvider.Initialize();
            _synchronizationProducer.Initialize();
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

        /// <summary>
        /// This function will work on arriving of every Synchronization Block and will initiate flow of checking whether current node is participating in Synchronization Group
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string SynchronizationGroupParticipationCheckAction(string arg)
        {
            _isParticipating = _nodesRatingProvider.IsCandidateInTopList(_accountState.AccountKey);

            if (_isParticipating)
            {
                _synchronozationGroupLeaderCheckUnsubscriber = _synchronizationGroupParticipationCheckAction.LinkTo(_synchronizationGroupLeaderCheckAction);
                _synchronizationGroupLeaderCheckAction.Post(null);
            }
            else
            {
                _synchronozationGroupLeaderCheckUnsubscriber.Dispose();
            }

            return arg;
        }

        private void SynchronizationGroupLeaderCheckAction(string arg)
        {
            bool recheckPosition = _synchronizationContext.LastBlockDescriptor == null || _synchronizationContext.LastBlockDescriptor.Round % _nodesRatingProvider.GetParticipantsCount() == 0;

            if (recheckPosition)
            {
                _ratingPosition = _nodesRatingProvider.GetCandidateRating(_accountState.AccountKey);
                _synchronizationProducer.DeferredBroadcast((ushort)_ratingPosition);
            }
        }

        #endregion Private Functions
    }

}

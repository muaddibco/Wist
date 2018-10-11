using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
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
        private readonly ILogger _logger;
        private readonly TransformBlock<string, string> _synchronizationGroupParticipationCheckAction;
        private readonly ActionBlock<string> _synchronizationGroupLeaderCheckAction;
        private readonly object _sync = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isParticipating;
        private bool _isStarted;
        private bool _roundStarted;
        private int _ratingPosition;
        private IDisposable _synchronozationGroupLeaderCheckUnsubscriber;

        public SynchronizationGroupParticipationService(ISynchronizationProducer synchronizationProducer, IStatesRepository statesRepository, 
            INodesRatingProviderFactory nodesRatingProvidersFactory, ILoggerService loggerService)
        {
            _synchronizationProducer = synchronizationProducer;
            _nodesRatingProvider = nodesRatingProvidersFactory.GetInstance(PacketType.Transactional);
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _synchronizationGroupParticipationCheckAction = new TransformBlock<string, string>((Func<string, string>)SynchronizationGroupParticipationCheckAction, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
            _synchronizationGroupLeaderCheckAction = new ActionBlock<string>((Action<string>)SynchronizationGroupLeaderCheckAction, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
            _logger = loggerService.GetLogger(nameof(SynchronizationGroupParticipationService));
        }

        public void Initialize()
        {
            _logger.Info("Initializing");
            try
            {
                _nodesRatingProvider.Initialize();
                _synchronizationProducer.Initialize();
            }
            catch (Exception ex)
            {
                _logger.Error("Initializing failed", ex);
            }
            _logger.Info("Initialized");
        }

        public void Start()
        {
            _logger.Info("Starting");
            if (_isStarted)
            {
                _logger.Info("Already started");
                return;
            }

            lock(_sync)
            {
                if(_isStarted)
                {
                    _logger.Info("Already started");
                    return;
                }

                _synchronizationContext.SubscribeOnStateChange(_synchronizationGroupParticipationCheckAction);

                _isStarted = true;

                _synchronizationGroupParticipationCheckAction.Post(null);
            }

            _logger.Info("Started");
        }

        public void Stop()
        {
            _logger.Info("Stopping");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _logger.Info("Stopped");
        }

        #region Private Functions

        /// <summary>
        /// This function will work on arriving of every Synchronization Block and will initiate flow of checking whether current node is participating in Synchronization Group
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string SynchronizationGroupParticipationCheckAction(string arg)
        {
            _logger.Info($"{nameof(SynchronizationGroupParticipationCheckAction)} starting");

            try
            {
                _roundStarted = false;
                _synchronizationProducer.CancelDeferredBroadcast();
                _isParticipating = _nodesRatingProvider.IsCandidateInTopList(_accountState.AccountKey);

                if (_isParticipating)
                {
                    _logger.Info($"{nameof(SynchronizationGroupParticipationCheckAction)} - participating");
                    _synchronozationGroupLeaderCheckUnsubscriber = _synchronizationGroupParticipationCheckAction.LinkTo(_synchronizationGroupLeaderCheckAction);
                    _synchronizationGroupLeaderCheckAction.Post(null);
                }
                else
                {
                    _logger.Info($"{nameof(SynchronizationGroupParticipationCheckAction)} - not participating");
                    _synchronozationGroupLeaderCheckUnsubscriber?.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(SynchronizationGroupParticipationCheckAction)} failed", ex);
            }

            return arg;
        }

        private void SynchronizationGroupLeaderCheckAction(string arg)
        {
            _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} starting");

            if (_roundStarted)
            {
                _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} - already started");
                return;
            }

            lock (_sync)
            {
                if(_roundStarted)
                {
                    _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} - already started");
                    return;
                }

                try
                {
                    bool recheckPosition = _synchronizationContext.LastBlockDescriptor == null || _synchronizationContext.LastBlockDescriptor.Round % _nodesRatingProvider.GetParticipantsCount() == 0;

                    if (recheckPosition)
                    {
                        _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} - deferred sync producing launched");
                        _roundStarted = true;
                        _ratingPosition = _nodesRatingProvider.GetCandidateRating(_accountState.AccountKey) + 1;
                        _synchronizationProducer.DeferredBroadcast((ushort)_ratingPosition, () => 
                        {
                            _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} - sync producing executed");
                            _synchronizationGroupParticipationCheckAction.Post(null);
                        });
                    }
                    else
                    {
                        _logger.Info($"{nameof(SynchronizationGroupLeaderCheckAction)} - sync producing not launched");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"{nameof(SynchronizationGroupLeaderCheckAction)} failed", ex);
                }
            }
        }

        #endregion Private Functions
    }

}

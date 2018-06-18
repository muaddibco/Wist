﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISynchronizationGroupParticipationService), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupParticipationService : ISynchronizationGroupParticipationService
    {
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly TransformBlock<string, string> _synchronizationGroupParticipationCheckAction;
        private readonly ActionBlock<string> _synchronizationGroupLeaderCheckAction;
        private readonly object _sync = new object();

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isParticipating;
        private bool _isStarted;
        private IDisposable _synchronozationGroupLeaderCheckUnsubscriber;

        public SynchronizationGroupParticipationService(ISynchronizationProducer synchronizationProducer, IStatesRepository statesRepository)
        {
            _synchronizationProducer = synchronizationProducer;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
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
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{

    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupState : ISynchronizationGroupState
    {
        public const string NAME = "SynchronizationGroupState";

        private readonly ConcurrentDictionary<IKey, IKey> _participants;
        private readonly Subject<string> _subject = new Subject<string>();

        public SynchronizationGroupState()
        {
            _participants = new ConcurrentDictionary<IKey, IKey>();
        }

        public string Name => NAME;

        public bool AddParticipant(IKey key)
        {
            if(!_participants.ContainsKey(key))
            {
                _participants.AddOrUpdate(key, key, (k, v) => v);
                return true;
            }

            return false;
        }

        public bool CheckParticipant(IKey key)
        {
            return _participants.ContainsKey(key);
        }

        public IEnumerable<IKey> GetAllParticipants()
        {
            return _participants.Values;
        }

        public bool RemoveParticipant(IKey key)
        {
            IKey temp;
            return _participants.TryRemove(key, out temp);
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}

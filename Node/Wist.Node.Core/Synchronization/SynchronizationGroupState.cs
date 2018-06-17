using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;

namespace Wist.Node.Core.Synchronization
{

    [RegisterExtension(typeof(SynchronizationGroupState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupState : ISynchronizationGroupState
    {
        public const string NAME = "SynchronizationGroupState";

        private readonly ConcurrentDictionary<IKey, IKey> _participants;

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
    }
}

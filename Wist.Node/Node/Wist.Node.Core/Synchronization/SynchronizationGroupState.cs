using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{

    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupState : NeighborhoodStateBase, ISynchronizationGroupState
    {
        public const string NAME = nameof(ISynchronizationGroupState);

        private readonly ConcurrentDictionary<IKey, IKey> _participants;
        private readonly Subject<string> _subject = new Subject<string>();

        public SynchronizationGroupState()
        {
            _participants = new ConcurrentDictionary<IKey, IKey>();
        }

        public override string Name => NAME;

        public bool CheckParticipant(IKey key)
        {
            return _neighbors.Contains(key);
        }
    }
}

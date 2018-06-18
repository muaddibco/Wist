using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;
using Wist.Core.States;

namespace Wist.Core.Communication
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    /// <summary>
    /// Class that contains general list of neighbor nodes
    /// </summary>
    public class NeighborhoodState : INeighborhoodState
    {
        public const string NAME = nameof(NeighborhoodState);

        private readonly Subject<string> _subject;

        private readonly HashSet<IKey> _neighbors;

        public NeighborhoodState()
        {
            _subject = new Subject<string>();
            _neighbors = new HashSet<IKey>();
        }

        public string Name => NAME;

        public bool AddNeighbor(IKey key)
        {
            bool res = _neighbors.Add(key);
            _subject.OnNext(null);
            return res;
        }

        public IEnumerable<IKey> GetAllNeighbors()
        {
            return _neighbors;
        }

        public bool RemoveNeighbor(IKey key)
        {
            bool res = _neighbors.Remove(key);
            _subject.OnNext(null);

            return res;
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}

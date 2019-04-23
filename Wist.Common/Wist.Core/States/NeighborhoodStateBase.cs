using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Core.States
{
    public abstract class NeighborhoodStateBase : INeighborhoodState
    {
        private readonly Subject<string> _subject;

        protected readonly HashSet<IKey> _neighbors;

        public NeighborhoodStateBase()
        {
            _subject = new Subject<string>();
            _neighbors = new HashSet<IKey>();
        }

        public abstract string Name { get; }

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

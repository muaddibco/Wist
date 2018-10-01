using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Common
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public const string NAME = nameof(INodeContext);

        private readonly Subject<string> _subject = new Subject<string>();

        public NodeContext()
        {
            SyncGroupParticipants = new List<SynchronizationGroupParticipant>();
        }

        public string Name => NAME;

        public List<SynchronizationGroupParticipant> SyncGroupParticipants { get; private set; }

        public void Initialize()
        {
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}

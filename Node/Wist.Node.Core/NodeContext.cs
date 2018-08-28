using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public const string NAME = nameof(NodeContext);

        private readonly Subject<string> _subject = new Subject<string>();

        public NodeContext()
        {
            SyncGroupParticipants = new List<ConsensusGroupParticipant>();
        }

        public IKey NodeKey { get; }

        public List<ConsensusGroupParticipant> SyncGroupParticipants { get; }

        public ushort SyncGroupParticipantsCount => 21;

        public string Name => NAME;

        public void Initialize()
        {
            
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }
    }
}

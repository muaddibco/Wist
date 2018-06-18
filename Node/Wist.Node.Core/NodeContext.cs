using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public const string NAME = "NodeState";

        private readonly Subject<string> _subject = new Subject<string>();

        public NodeContext()
        {
            SyncGroupParticipants = new List<ConsensusGroupParticipant>();

            //TODO: set PublicKey
            ThisNode = new ConsensusGroupParticipant() { };
        }

        public byte[] PublicKey { get; }

        public ConsensusGroupParticipant ThisNode { get; }
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

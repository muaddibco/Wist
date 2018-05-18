using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core
{
    [RegisterDefaultImplementation(typeof(INodeContext), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public NodeContext(ISynchronizationContext synchronizationContext)
        {
            SynchronizationContext = synchronizationContext;
            SyncGroupParticipants = new List<ConsensusGroupParticipant>();

            // TODO: set PublicKey
            ThisNode = new ConsensusGroupParticipant() { };
        }

        public byte[] PublicKey { get; }

        public ConsensusGroupParticipant ThisNode { get; }
        public List<ConsensusGroupParticipant> SyncGroupParticipants { get; }
        public ISynchronizationContext SynchronizationContext { get; }

        public void Initialize()
        {
            
        }

        public byte[] Sign(byte[] message)
        {
            return CryptoHelper.ComputeHash(message);
        }
    }
}

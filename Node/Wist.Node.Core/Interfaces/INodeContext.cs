using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    public interface INodeContext : IState
    {
        byte[] PublicKey { get; }

        ConsensusGroupParticipant ThisNode { get; }

        ushort SyncGroupParticipantsCount { get; }

        /// <summary>
        /// Complete list of current participants involved into producing synchronization blocks
        /// </summary>
        List<ConsensusGroupParticipant> SyncGroupParticipants { get; }

        ISynchronizationContext SynchronizationContext { get; }

        void Initialize();

        /// <summary>
        /// Signs message using Private Key of current Node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Sign(byte[] message);
    }
}

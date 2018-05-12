using System;
using System.Collections.Generic;
using System.Text;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    public interface INodeContext
    {
        byte[] PublicKey { get; }

        ConsensusGroupParticipant ThisNode { get; set; }

        void Initialize();

        /// <summary>
        /// Signs message using Private Key of current Node
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        byte[] Sign(byte[] message);
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface ISynchronizationProducer
    {
        void Initialize(ICommunicationServer communicationHub);

        /// <summary>
        /// Function checks for last obtained synchronization block, checks at what time it was obtained and 60 seconds later it invokes sending new synchronization block for confirmation
        /// </summary>
        void DeferredBroadcast();
    }
}

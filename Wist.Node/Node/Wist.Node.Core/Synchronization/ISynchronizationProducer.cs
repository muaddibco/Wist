using System;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISynchronizationProducer
    {
        void Initialize();

        /// <summary>
        /// Function checks for last obtained synchronization block, checks at what time it was obtained and 60 seconds later it invokes sending new synchronization block for confirmation
        /// </summary>
        void DeferredBroadcast(ushort round, Action onBroadcasted);
        void CancelDeferredBroadcast();
    }
}

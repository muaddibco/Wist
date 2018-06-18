using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISynchronizationGroupParticipationService
    {
        void Initialize();

        void Start();

        void Stop();
    }
}

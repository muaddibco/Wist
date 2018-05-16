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
        void Initialize(ICommunicationHub communicationHub);

        void Launch();
    }
}

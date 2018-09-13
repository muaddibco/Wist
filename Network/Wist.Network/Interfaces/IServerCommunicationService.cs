using Wist.Network.Communication;
using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using System.Net;
using Wist.Core.Identity;

namespace Wist.Network.Interfaces
{
    [ExtensionPoint]
    public interface IServerCommunicationService : ICommunicationService
    {
        void InitCommunicationProvisioning(ICommunicationProvisioning communicationProvisioning = null);

        void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation);
    }
}

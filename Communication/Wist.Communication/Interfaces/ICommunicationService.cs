using Wist.Communication.Sockets;
using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Models;
using Wist.Core.Communication;
using System.Net;

namespace Wist.Communication.Interfaces
{
    [ExtensionPoint]
    public interface ICommunicationService
    {
        string Name {get;}
        
        void Init(SocketListenerSettings settings, ICommunicationProvisioning communicationProvisioning = null);

        void Stop();

        void Start();

        void PostMessage(IKey destination, IPacketProvider message);
        void PostMessage(IEnumerable<IKey> destinations, IPacketProvider message);

        void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation);
    }
}

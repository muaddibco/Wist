using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Communication.Interfaces
{
    [ExtensionPoint]
    public interface ICommunicationService
    {
        string Name { get; }

        void Stop();

        void Start();
        void Init(SocketSettings settings);

        void PostMessage(IKey destination, IPacketProvider message);

        void PostMessage(IEnumerable<IKey> destinations, IPacketProvider message);
    }
}

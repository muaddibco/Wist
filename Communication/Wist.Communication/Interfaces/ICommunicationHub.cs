using Wist.Communication.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.BlockLattice.Core.DataModel;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface ICommunicationHub
    {
        void Init(SocketListenerSettings settings, ICommunicationProvisioning communicationProvisioning = null);

        void StartListen();

        void BroadcastMessage(BlockBase message);
    }
}

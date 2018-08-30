using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Communication;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface ICommunicationChannel : IDisposable
    {
        event EventHandler<EventArgs> SocketClosedEvent;

        IPAddress RemoteIPAddress { get; }

        void PushForParsing(byte[] buf, int offset, int count);

        void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler);

        void RegisterExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceivedExtendedValidation);

        void Stop();

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(byte[] message);
    }
}

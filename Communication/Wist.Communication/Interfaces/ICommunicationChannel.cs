using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Communication;

namespace Wist.Communication.Interfaces
{
    public interface ICommunicationChannel
    {
        event EventHandler<EventArgs> SocketClosedEvent;

        int TokenId { get; }

        IPAddress RemoteIPAddress { get; }

        Queue<byte[]> MessagePackets { get; }

        void PushForParsing(byte[] buf, int count);

        IEnumerable<byte[]> GetMessagesToSend();

        void Init(int tokenId, int sendReceiveBufferSize, bool keepAlive, IPacketsHandler packetsHandler);

        void Stop();

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(IMessage message);
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;

namespace Wist.Communication.Interfaces
{
    public interface ICommunicationChannel
    {
        event EventHandler<EventArgs> SocketClosedEvent;

        int TokenId { get; }

        IPEndPoint RemoteEndPoint { get; }

        Queue<byte[]> MessagePackets { get; }

        void PushForParsing(byte[] buf, int count);

        IEnumerable<byte[]> GetMessagesToSend();

        void Init(int tokenId, int sendReceiveBufferSize, bool keepAlive, IPacketsHandler packetsHandler);

        void Stop();

        void Connect(EndPoint endPoint);

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(byte[] message);
    }
}

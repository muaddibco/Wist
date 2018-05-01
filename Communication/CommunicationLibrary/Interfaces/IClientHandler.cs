using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary.Interfaces
{
    public interface IClientHandler
    {
        event EventHandler<EventArgs> SocketClosedEvent;

        int TokenId { get; }

        IPEndPoint RemoteEndPoint { get; }

        Queue<byte[]> MessagePackets { get; }

        void PushBuffer(byte[] buf, int count);

        IEnumerable<byte[]> GetMessagesToSend();

        void Init(int tokenId, int sendReceiveBufferSize, bool keepAlive);

        void Stop();

        void Connect(EndPoint endPoint);

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(byte[] message);
    }
}

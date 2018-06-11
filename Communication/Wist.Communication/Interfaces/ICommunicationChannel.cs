﻿using System;
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

        IPAddress RemoteIPAddress { get; }

        Queue<byte[]> MessagePackets { get; }

        void PushForParsing(byte[] buf, int count);

        void Init(IBufferManager bufferManager, IPacketsHandler packetsHandler, Action<ICommunicationChannel, int> onReceivedAction = null);

        void Stop();

        void Close();

        void AcceptSocket(Socket acceptSocket);

        void PostMessage(IMessage message);
    }
}
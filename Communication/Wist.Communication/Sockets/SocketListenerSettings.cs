using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Wist.Communication.Sockets
{
    public class SocketListenerSettings
    {
        /// <summary>
        /// the maximum number of connections the server is designed to handle simultaneously 
        /// </summary>
        public int MaxConnections { get; }

        /// <summary>
        /// buffer size to use for each socket receive operation
        /// </summary>
        public int ReceiveBufferSize { get; }

        /// <summary>
        /// Endpoint for the listener
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; }

        public SocketListenerSettings(int maxConnections, int receiveBufferSize, IPEndPoint listeningEndpoint)
        {
            MaxConnections = maxConnections;
            ReceiveBufferSize = receiveBufferSize;
            ListeningEndpoint = listeningEndpoint;
        }
    }
}

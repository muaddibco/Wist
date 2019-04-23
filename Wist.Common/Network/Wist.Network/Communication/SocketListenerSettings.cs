using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Wist.Network.Communication
{
    public class SocketListenerSettings : SocketSettings
    {
        /// <summary>
        /// Endpoint for the listener
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; }

        public SocketListenerSettings(int maxConnections, int receiveBufferSize, IPEndPoint listeningEndpoint) 
            : base(maxConnections, receiveBufferSize, listeningEndpoint.Port, listeningEndpoint.AddressFamily)
        {
            ListeningEndpoint = listeningEndpoint;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CommunicationLibrary.Sockets
{
    public class SocketListenerSettings
    {
        /// <summary>
        /// the maximum number of connections the server is designed to handle simultaneously 
        /// </summary>
        public int MaxConnections { get; }

        /// <summary>
        /// max # of pending connections the listener can hold in queue
        /// </summary>
        public int MaxPendingConnections { get; }

        /// <summary>
        /// tells us how many objects to put in pool for accept operations
        /// </summary>
        public int MaxSimultaneousAcceptOps { get; }

        /// <summary>
        /// buffer size to use for each socket receive operation
        /// </summary>
        public int ReceiveBufferSize { get; }

        /// <summary>
        /// 
        /// </summary>
        public int OpsToPreAllocate { get; }

        /// <summary>
        /// Endpoint for the listener
        /// </summary>
        public IPEndPoint ListeningEndpoint { get; }

        public bool KeepAlive { get; set; }

        public SocketListenerSettings(int maxConnections, int maxPendingConnections, int maxSimultaneousAcceptOps, int receiveBufferSize, int opsToPreAllocate, IPEndPoint listeningEndpoint, bool keepAlive)
        {
            MaxConnections = maxConnections;
            MaxPendingConnections = maxPendingConnections;
            MaxSimultaneousAcceptOps = maxSimultaneousAcceptOps;
            ReceiveBufferSize = receiveBufferSize;
            OpsToPreAllocate = opsToPreAllocate;
            ListeningEndpoint = listeningEndpoint;
            KeepAlive = keepAlive;
        }
    }
}

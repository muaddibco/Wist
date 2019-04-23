using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Wist.Network.Communication
{
    public class SocketSettings
    {
        public SocketSettings(int maxConnections, int bufferSize, int remotePort, AddressFamily addressFamily)
        {
            MaxConnections = maxConnections;
            BufferSize = bufferSize;
            RemotePort = remotePort;
            AddressFamily = addressFamily;
        }

        /// <summary>
        /// the maximum number of connections the server is designed to handle simultaneously 
        /// </summary>
        public int MaxConnections { get; }

        /// <summary>
        /// buffer size to use for each socket receive operation
        /// </summary>
        public int BufferSize { get; }

        public int RemotePort { get; set; }

        public AddressFamily AddressFamily { get; set; }
    }
}

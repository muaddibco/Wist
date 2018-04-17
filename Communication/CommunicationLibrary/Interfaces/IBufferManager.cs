using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    [ServiceContract]
    public interface IBufferManager
    {
        void InitBuffer(int totalBytes, int bufferSize);
        bool SetBuffer(SocketAsyncEventArgs args);
        void FreeBuffer(SocketAsyncEventArgs args);
    }
}

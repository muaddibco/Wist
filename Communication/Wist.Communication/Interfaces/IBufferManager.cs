using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface IBufferManager
    {
        int BufferSize { get; }

        void InitBuffer(int totalBytes, int bufferSize);
        bool SetBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs);
        void FreeBuffer(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs);
    }
}

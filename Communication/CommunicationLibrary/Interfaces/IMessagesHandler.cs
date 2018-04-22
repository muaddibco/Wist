using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    /// <summary>
    /// Service receives raw arrays of bytes representing all types of messages exchanges over network. 
    /// Byte arrays must contain exact bytes of message to be processed correctly.
    /// </summary>
    [ServiceContract]
    public interface IMessagesHandler
    {
        void Push(byte[] messagePacket);

        void Start(bool withErrorsProcessing = true);

        void Stop();
    }
}

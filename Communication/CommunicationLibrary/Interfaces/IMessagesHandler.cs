using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    [ServiceContract]
    public interface IMessagesHandler
    {
        void Push(byte[] messagePacket);

        void Start(bool withErrorsProcessing = true);

        void Stop();
    }
}

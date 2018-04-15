using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Interfaces
{
    public interface IClientHandler
    {
        void PushBuffer(byte[] buf);

        IEnumerable<byte[]> GetMessagesToSend();

        void Start();

        void Stop();
    }
}

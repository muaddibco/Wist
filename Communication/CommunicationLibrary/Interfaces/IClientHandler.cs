using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Interfaces
{
    public interface IClientHandler
    {
        Queue<byte[]> MessagePackets { get; }

        void PushBuffer(byte[] buf, int count);

        IEnumerable<byte[]> GetMessagesToSend();

        void Start();

        void Stop();
    }
}

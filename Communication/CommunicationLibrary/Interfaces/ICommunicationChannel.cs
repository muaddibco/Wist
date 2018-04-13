using DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Interfaces
{
    public interface ICommunicationChannel
    {
        int Count { get; }

        MessageBase Pop();

        void Push(MessageBase message);
    }
}

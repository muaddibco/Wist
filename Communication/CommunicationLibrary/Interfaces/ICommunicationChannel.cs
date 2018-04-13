using Wist.Core.Models;

namespace CommunicationLibrary.Interfaces
{
    public interface ICommunicationChannel
    {
        int Count { get; }

        MessageBase Pop();

        void Push(MessageBase message);
    }
}

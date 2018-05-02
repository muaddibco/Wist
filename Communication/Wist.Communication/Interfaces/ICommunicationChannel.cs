using Wist.Core.Models;

namespace Wist.Communication.Interfaces
{
    public interface ICommunicationChannel
    {
        int Count { get; }

        MessageBase Pop();

        void Push(MessageBase message);
    }
}

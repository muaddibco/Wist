using Wist.Client.Common.Entities;

namespace Wist.Client.Common.Interfaces
{
    public interface INetworkManager
    {
        void InitializeNetwork();

        ulong GetCurrentHeight(Account account);
    }
}

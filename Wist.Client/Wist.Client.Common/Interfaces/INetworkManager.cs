using Wist.Client.Common.Entities;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Client.Common.Interfaces
{
    [ServiceContract]
    public interface INetworkManager
    {
        void InitializeNetwork();

        ulong GetCurrentHeight(Account account);

        bool SendBlock(byte[] data, Account account, IKey targetKey);
    }
}
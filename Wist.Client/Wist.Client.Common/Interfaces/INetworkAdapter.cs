using System.Collections.Generic;
using System.Net;

namespace Wist.Client.Common.Interfaces
{
    public interface INetworkAdapter
    {
        ICollection<IPAddress> GetIPAddressesOfStorageEndpoints();
        ICollection<IPAddress> GetIPAddressesOfRegistrationEndpoints();
        ulong GetCurrentHeightOfAccount(byte[] privateKey);
    }
}

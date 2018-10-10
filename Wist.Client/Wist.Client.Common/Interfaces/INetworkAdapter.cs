using System.Collections.Generic;
using System.Net;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Client.Common.Interfaces
{
    [ServiceContract]
    public interface INetworkAdapter
    {
        ICollection<IPAddress> GetIPAddressesOfStorageEndpoints();
        ICollection<IPAddress> GetIPAddressesOfRegistrationEndpoints();
        void SendBlock(byte[] data, IKey privateKey, IKey targetKey);
    }
}

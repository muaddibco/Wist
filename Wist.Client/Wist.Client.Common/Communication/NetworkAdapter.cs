using Grpc.Core;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Wist.Client.Common.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(INetworkAdapter), Lifetime = LifetimeManagement.Singleton)]
    public class NetworkAdapter :  INetworkAdapter
    {
        private INetworkSynchronizer _networkSynchronizer;

        private IDictionary<IPAddress, bool> _sentAckDictionary;

        public NetworkAdapter(INetworkSynchronizer networkSynchronizer)
        {
            _networkSynchronizer = networkSynchronizer;

            _sentAckDictionary = new Dictionary<IPAddress, bool>();
        }

        #region ============ PUBLIC FUNCTIONS =============  

        /// <summary>
        /// returns the remote ip's for sending the encrypted packet
        /// </summary>
        /// <returns></returns>
        public ICollection<IPAddress> GetIPAddressesOfStorageEndpoints()
        {
            return new List<IPAddress>
            {
                new IPAddress(192),
                new IPAddress(192),
                new IPAddress(192)
            };
        }

        /// <summary>
        /// returns the remote ip's for sending the encrypted packet
        /// </summary>
        /// <returns></returns>
        public ICollection<IPAddress> GetIPAddressesOfRegistrationEndpoints()
        {
            return new List<IPAddress>
            {
                new IPAddress(192),
                new IPAddress(192),
                new IPAddress(192)
            };
        }

        public ulong GetCurrentHeightOfAccount(byte[] privateKey)
        {
            throw new System.NotImplementedException();
        }

        public void SendBlock(byte[] data, IKey privateKey, IKey targetKey)
        {
            Parallel.ForEach(GetIPAddressesOfStorageEndpoints(), (ipAddress) =>
            {
                _networkSynchronizer.SendData(ipAddress, 5050, ChannelCredentials.Insecure, privateKey, targetKey);
            });   
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

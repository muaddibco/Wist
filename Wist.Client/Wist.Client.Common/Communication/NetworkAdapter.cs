using System.Collections.Generic;
using System.Net;
using Wist.Client.Common.Interfaces;

namespace Wist.Client.Common.Communication
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/29/2018 9:09:15 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// Network adapter for outside services from wist core projects
    /// </summary>
    public class NetworkAdapter :  INetworkAdapter
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        private INetworkSynchronizer _networkSynchronizer;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public NetworkAdapter(INetworkSynchronizer networkSynchronizer)
        {
            _networkSynchronizer = networkSynchronizer;
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

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

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

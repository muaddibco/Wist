using System.Collections.Generic;
using System.Net;
using Wist.Client.Common.Entities;
using Wist.Client.Common.Interfaces;

namespace Wist.Client.Common.Communication
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/30/2018 3:32:47 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class NetworkManager : INetworkManager
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        private INetworkAdapter _networkAdapter;

        private IDictionary<byte[], ulong> _heightsDictionary;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public NetworkManager(INetworkAdapter networkAdapter)
        {
            _networkAdapter = networkAdapter;
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public ICollection<IPAddress> StorageEndpoints { get; set; }

        public ICollection<IPAddress> RegistrationEndpoints { get; set; }

        public void InitializeNetwork()
        {
            StorageEndpoints = _networkAdapter.GetIPAddressesOfStorageEndpoints();

            RegistrationEndpoints = _networkAdapter.GetIPAddressesOfRegistrationEndpoints();
        }

        public ulong GetCurrentHeight(Account account)
        {
            return _heightsDictionary[account.PrivateKey];
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Wist.Client.Common.Entities;
using Wist.Client.Common.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

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
    [RegisterDefaultImplementation(typeof(INetworkManager), Lifetime = LifetimeManagement.Singleton)]
    public class NetworkManager : INetworkManager
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        private INetworkAdapter _networkAdapter;

        private readonly IDictionary<IKey, ulong> _heightsDictionary;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public NetworkManager(INetworkAdapter networkAdapter)
        {
            _networkAdapter = networkAdapter;

            _heightsDictionary = new Dictionary<IKey, ulong>();
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public void InitializeNetwork()
        {
           
        }

        public ulong GetCurrentHeight(Account account)
        {
            return _heightsDictionary[account.PrivateKey];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SendBlock(byte[] data, Account account, IKey targetKey)
        {
            _networkAdapter.SendBlock(data, account.Key, targetKey);
            
            return true;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

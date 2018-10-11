using System;
using System.Collections.Generic;
using Wist.Client.Common.Entities;
using Wist.Client.Common.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
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

        private ICryptoService _cryptoService;

        private readonly IDictionary<byte[], ulong> _heightsDictionary;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public NetworkManager(INetworkAdapter networkAdapter, ICryptoService cryptoService)
        {
            _networkAdapter = networkAdapter;

            _cryptoService = cryptoService;

            _heightsDictionary = new Dictionary<byte[], ulong>();
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

        public bool SendBlock(byte[] data, Account account, byte[] targetKey)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Client.Common.Entities;
using Wist.Client.Common.Interfaces;
using WistWallet.Base.Interfaces;

namespace WistWallet.Base.Services
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/29/2018 10:15:51 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class Context : IContext
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        protected INetworkManager _networkManager;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public Context()
        {
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

     

        public ICollection<Account> WalletAccounts { get; set; }

        public ulong GetLatestBlockHeight(Account account)
        {
            if (WalletAccounts.ToList().Any(t => t.PrivateKey == account.PrivateKey))
            {
                _networkManager.GetCurrentHeight(account);
            }
            else
            {
                throw new Exception("this account is part of current wallet");
            }
            return 0;
        }

        public virtual INetworkManager GetNetworkManager()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

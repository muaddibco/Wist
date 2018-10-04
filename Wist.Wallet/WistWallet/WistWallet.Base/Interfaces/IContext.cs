using System.Collections.Generic;
using Wist.Client.Common.Entities;
using Wist.Client.Common.Interfaces;

namespace WistWallet.Base.Interfaces
{
    public interface IContext
    {
        ICollection<Account> WalletAccounts { get; set; }
        ulong GetLatestBlockHeight(Account account);
        INetworkManager GetNetworkManager();
    }
}

using System;

namespace WistWallet.Base.Models
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/13/2018 1:25:10 AM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// Self identity model for the wallet
    /// </summary>
    public class Identity
    {
        public Guid PrivateKey { get; set; }

        public Guid PublicKey { get; set; }

        public string WalletName { get; set; }

        public string PhoneNumber { get; set; }
    }
}

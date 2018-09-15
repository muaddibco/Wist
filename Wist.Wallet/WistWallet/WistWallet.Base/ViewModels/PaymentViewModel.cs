using System;
using System.Collections.Generic;
using System.Linq;
using WistWallet.Base.Interfaces;
using WistWallet.Base.Models;
using WistWallet.Base.Services;

namespace WistWallet.Base.ViewModels
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/15/2018 1:41:15 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class PaymentViewModel : IPaymentViewModel
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        public string LabelTitle => Strings.PaymentTitle;
        public string LabelSelectedCurrency => Strings.SelectedCurrency;
        public string LabelSelectedSum => Strings.SelectedSum;
        public string LabelTargetUser => Strings.TargetUser;
        public string LabelPay => Strings.Pay;

        public Currency SelectedCurrency { get; set; }
        public uint SelectedSum { get; set; }
        public string SelectedUser { get; set; }
        public ICollection<string> ListCurrency => Enum.GetNames(typeof(Currency)).ToList();

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public PaymentViewModel()
        {
            SelectedSum = 0;

            SelectedCurrency = Currency.UsDollar;
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public void SendPayment()
        {

        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

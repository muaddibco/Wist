using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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
    public class PaymentViewModel : BaseViewModel, IPaymentViewModel
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        public string LabelTitle => Strings.PaymentTitle;
        public string LabelSelectedCurrency => Strings.SelectedCurrency;
        public string LabelSelectedSum => Strings.SelectedSum;
        public string LabelTargetUser => Strings.TargetUser;
        public string LabelPay => Strings.Pay;

        public uint SelectedSum { get; set; }
        public string SelectedUser { get; set; }
        public Currency SelectedCurrency { get; set; }
        public ICollection<string> ListCurrency => Enum.GetNames(typeof(Currency)).ToList();
        public virtual ICommand SendPaymentCommand { get; }

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

        

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

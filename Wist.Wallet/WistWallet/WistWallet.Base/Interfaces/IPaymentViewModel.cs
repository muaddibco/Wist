using System.Collections.Generic;
using System.Windows.Input;
using WistWallet.Base.Models;

namespace WistWallet.Base.Interfaces
{
    public interface IPaymentViewModel
    {
        string LabelSelectedCurrency { get; }
        string LabelSelectedSum { get; }
        string LabelTargetUser { get; }
        string LabelPay { get; }
        string LabelTitle { get; }
        string SelectedUser { get; set; }
        uint SelectedSum { get; set; }
        Currency SelectedCurrency { get; set; }
        ICommand SendPaymentCommand { get; }
        ICollection<string> ListCurrency { get; }
    }
}
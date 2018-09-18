using System.Windows.Input;
using WistWallet.Base.Models;

namespace WistWallet.Base.Interfaces
{
    public interface IPaymentViewModel
    {
        string LabelTitle { get; }

        Currency SelectedCurrency { get; set; }

        uint SelectedSum { get; set; }

        string SelectedUser { get; set; }

        ICommand SendPaymentCommand { get; }
    }
}

using WistWallet.Base.Interfaces;
using Xamarin.Forms;

namespace WistWallet.Base.Mobile.Interfaces
{
    public interface IVoteViewModelMobile : IVoteViewModel
    {
        Command LoadItemsCommand { get; set; }
    }
}

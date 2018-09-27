using System.Windows.Input;
using WistWallet.Base.Models;

namespace WistWallet.Base.Interfaces
{
    public interface IVoteViewModel
    {
        VotePoll VotePoll { get; set; }

        ICommand LoadItemsCommand { get; set; }
    }
}

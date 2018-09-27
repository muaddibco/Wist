using System.Collections.ObjectModel;
using WistWallet.Base.Mobile.Interfaces;
using WistWallet.Base.Models;
using WistWallet.Base.ViewModels;
using Xamarin.Forms;

namespace WistWallet.Base.Mobile.ViewModels
{
    public class VoteViewModelMobile : VoteViewModel, IVoteViewModelMobile
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public VoteViewModelMobile() : base()
        {
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        private void ExecuteLoadItemsCommand()
        {
            VotePoll = new VotePoll()
            {
                Description = "Poly",
                OpenForVoting = true,
                Votings = new ObservableCollection<VoteItem>
                {
                    new VoteItem(){ IsSelected = true, Label = "Bruno"},
                    new VoteItem(){ IsSelected = true, Label = "Romario.F"},
                    new VoteItem(){ IsSelected = true, Label = "Ronaldo"},
                    new VoteItem(){ IsSelected = true, Label = "Ronaldinho"}
                }
            };
        }
    }
}
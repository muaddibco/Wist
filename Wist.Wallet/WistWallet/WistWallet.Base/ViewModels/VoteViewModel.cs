using System.Collections.ObjectModel;
using System.Windows.Input;
using WistWallet.Base.Interfaces;
using WistWallet.Base.Models;

namespace WistWallet.Base.ViewModels
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/15/2018 10:57:18 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class VoteViewModel : BaseViewModel, IVoteViewModel
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        public VotePoll VotePoll { get; set; }

        public ICommand LoadItemsCommand { get; set; }

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public VoteViewModel()
        {
            //VotePoll = new VotePoll()
            //{
            //    Description = "Poly",
            //    OpenForVoting = true,
            //    Votings = new ObservableCollection<VoteItem>
            //    {
            //        new VoteItem(){ IsSelected = true, Label = "Bruno"},
            //        new VoteItem(){ IsSelected = true, Label = "Romario.F"},
            //        new VoteItem(){ IsSelected = true, Label = "Ronaldo"},
            //        new VoteItem(){ IsSelected = true, Label = "Ronaldinho"}
            //    }
            //};
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

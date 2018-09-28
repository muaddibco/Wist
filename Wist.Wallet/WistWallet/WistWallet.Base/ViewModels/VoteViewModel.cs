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

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public VoteViewModel()
        {
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

using System;
using System.Collections.Generic;
using System.Text;

namespace WistWallet.Base.Models
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/15/2018 11:00:43 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class VotePoll
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        public string Description { get; set; }

        public ICollection<VoteItem> Votings { get; set; }

        public bool OpenForVoting { get; set; }

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public VotePoll()
        {
            Votings = new List<VoteItem>();
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

using RestSharp;
using Wist.Common.Interfaces;

namespace Wist.Common.Communication
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/15/2018 10:26:06 AM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// bla bla
    /// </summary>
    public class RestfulClient : IRestfulClient
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        private readonly RestClient _restClient;

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public RestfulClient()
        {
            _restClient = new RestClient("baseUrl");
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

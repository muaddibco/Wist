using CommonServiceLocator;
using Wist.Client.Common.Interfaces;
using WistWallet.Base.Mobile.Interfaces;
using WistWallet.Base.Mobile.Services;
using WistWallet.Base.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(MobileContext))]
namespace WistWallet.Base.Mobile.Services
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 10/5/2018 12:13:08 AM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class MobileContext : Context, IMobileContext
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================


        //============================================================================
        //                                  C'TOR
        //============================================================================

        public MobileContext() 
            : base()
        {

        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public override INetworkManager GetNetworkManager()
        {
            if (_networkManager == null)
            {
                _networkManager = ServiceLocator.Current.GetInstance<INetworkManager>();
            }
            return _networkManager;
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

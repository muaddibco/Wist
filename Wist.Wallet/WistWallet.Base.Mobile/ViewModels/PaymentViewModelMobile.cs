using CommonServiceLocator;
using System.Linq;
using System.Windows.Input;
using Wist.Client.Common.Interfaces;
using WistWallet.Base.Interfaces;
using WistWallet.Base.Mobile.Interfaces;
using WistWallet.Base.ViewModels;
using Xamarin.Forms;

namespace WistWallet.Base.Mobile.ViewModels
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/18/2018 1:29:37 AM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class PaymentViewModelMobile : PaymentViewModel , IPaymentViewModel
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================


        //============================================================================
        //                                  C'TOR
        //============================================================================

        public PaymentViewModelMobile(): base()
        {

        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public override ICommand SendPaymentCommand =>
            new Command(() =>
            {
                byte[] test = new byte[] { 1, 1 };

                DependencyService.Get<INotificationService>().ShowMessage("Heeeeeeeeeeeello");

                IMobileContext mobileContext = DependencyService.Get<IMobileContext>();

                INetworkManager networkManager = mobileContext?.GetNetworkManager();

                ulong height = networkManager.GetCurrentHeight(mobileContext.WalletAccounts.ToList().FirstOrDefault(a => a.PrivateKey == test));
            });

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

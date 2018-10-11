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
                byte[] privateKey = new byte[] { 1, 1 };

                DependencyService.Get<INotificationService>().ShowMessage("Heeeeeeeeeeeello");

                IMobileContext mobileContext = DependencyService.Get<IMobileContext>();

                INetworkManager networkManager = mobileContext?.GetNetworkManager();

                byte[] data = new byte[] { 1, 1, 1 };

                networkManager.SendBlock(data, mobileContext.WalletAccounts.ToList().FirstOrDefault(k => k.PrivateKey == privateKey), privateKey);
            });

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

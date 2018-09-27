using Android.Content;
using Android.Widget;
using System.Threading.Tasks;
using WistWallet.Base.Mobile.Interfaces;
using WistWallet.Droid.Services;
using Xamarin.Forms;

[assembly:Dependency(typeof(NotificationService))]
namespace WistWallet.Droid.Services
{

    public class NotificationService : INotificationService
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================

        public Context Context { get; set; }

        //============================================================================
        //                                  C'TOR
        //============================================================================

        public NotificationService()
        {

        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        /// <summary>
        /// Show immediate message on screen
        /// </summary>
        /// <param name="msg">The message to be shown</param>
        /// <param name="longMessage">is the message should remain longer on screen?</param>
        /// <param name="asyncCall">is this message should run as async task?</param>
        public void ShowMessage(string msg, bool longMessage = false, bool asyncCall = false)
        {
            if (asyncCall)
            {
                Task.Run(() =>
                {
                    Toast.MakeText(Context, msg, longMessage ? ToastLength.Long : ToastLength.Short).Show();
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(Context, msg, longMessage ? ToastLength.Long : ToastLength.Short).Show();
                });
            }
        }

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}
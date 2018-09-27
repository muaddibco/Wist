using Android.App;
using Android.Content.PM;
using Android.OS;
using WistWallet.Base.Mobile;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using WistWallet.Base.Mobile.Interfaces;
using Xamarin.Forms;
using WistWallet.Droid.Services;

namespace WistWallet.Droid
{
    [Activity(Label = "WistWallet", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            AppCenter.Start("8d5c4029-c35e-4900-8618-1928d913a2d0", typeof(Analytics), typeof(Crashes));
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Forms.Init(this, bundle);
            ((NotificationService)DependencyService.Get<INotificationService>()).Context = this;
            LoadApplication(new App());
        }
    }
}


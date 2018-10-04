using Xamarin.Forms;
using WistWallet.Base.Mobile.Views;
using Xamarin.Forms.Xaml;
using GalaSoft.MvvmLight.Ioc;
using CommonServiceLocator;
using Wist.Client.Common.Interfaces;
using Wist.Client.Common.Communication;
using WistWallet.Base.Mobile.Interfaces;
using WistWallet.Base.Mobile.Services;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace WistWallet.Base.Mobile
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<INetworkAdapter, NetworkAdapter>();
            SimpleIoc.Default.Register<INetworkManager, NetworkManager>();
            SimpleIoc.Default.Register<INetworkSynchronizer, NetworkSynchronizer>();
            SimpleIoc.Default.Register<MobileContext>();
        }

        protected override void OnStart()
        {
            MainPage = new MainPage();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}

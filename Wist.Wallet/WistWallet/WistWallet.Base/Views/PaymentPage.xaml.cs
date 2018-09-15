using WistWallet.Base.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WistWallet.Base.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PaymentPage : ContentPage
	{
        public IPaymentViewModel PaymentViewModel { get; set; }

        public PaymentPage ()
		{
			InitializeComponent ();
		}
	}
}
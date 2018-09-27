namespace WistWallet.Base.Mobile.Interfaces
{
    public interface INotificationService
    {
        void ShowMessage(string msg, bool longMessage = false, bool asyncCall = false);
    }
}

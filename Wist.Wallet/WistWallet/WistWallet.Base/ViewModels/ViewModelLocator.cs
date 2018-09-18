using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using WistWallet.Base.Interfaces;

namespace WistWallet.Base.ViewModels
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/15/2018 1:30:47 PM      
    /// *****************************************************************/
    /// </classDetails>
    /// <summary>
    /// </summary>
    public class ViewModelLocator
    {
        //============================================================================
        //                                 MEMBERS
        //============================================================================


        //============================================================================
        //                                  C'TOR
        //============================================================================

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<IPaymentViewModel, PaymentViewModel>();

            SimpleIoc.Default.Register<IVoteViewModel, VoteViewModel>();
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public IPaymentViewModel PaymentViewModel => ServiceLocator.Current.GetInstance<IPaymentViewModel>();
        public IVoteViewModel VotePaymentViewModel => ServiceLocator.Current.GetInstance<IVoteViewModel>();

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

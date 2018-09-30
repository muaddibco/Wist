﻿using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using Wist.Client.Common.Communication;
using Wist.Client.Common.Interfaces;
using Wist.Common.Communication;
using Wist.Common.Interfaces;
using WistWallet.Base.Interfaces;
using WistWallet.Base.Mobile.Interfaces;

namespace WistWallet.Base.Mobile.ViewModels
{
    /// <classDetails>   
    ///*****************************************************************
    ///  Machine Name : AMI-PC
    /// 
    ///  Author       : Ami
    ///       
    ///  Date         : 9/27/2018 1:10:47 AM      
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

            SimpleIoc.Default.Register<IPaymentViewModel, PaymentViewModelMobile>();
            SimpleIoc.Default.Register<IVoteViewModelMobile, VoteViewModelMobile>();
            SimpleIoc.Default.Register<INetworkAdapter, NetworkAdapter>();
            SimpleIoc.Default.Register<INetworkManager, NetworkManager>();
            SimpleIoc.Default.Register<INetworkSynchronizer, NetworkSynchronizer>();
            SimpleIoc.Default.Register<IRestfulClient, RestfulClient>();
        }

        //============================================================================
        //                                FUNCTIONS
        //============================================================================

        #region ============ PUBLIC FUNCTIONS =============  

        public IPaymentViewModel PaymentViewModel => ServiceLocator.Current.GetInstance<IPaymentViewModel>();
        public IVoteViewModelMobile VoteViewModel => ServiceLocator.Current.GetInstance<IVoteViewModelMobile>();

        #endregion

        #region ============ PRIVATE FUNCTIONS ============ 


        #endregion

    }
}

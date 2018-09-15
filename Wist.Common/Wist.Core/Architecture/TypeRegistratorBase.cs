using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using Wist.Core.Architecture.Registration;

namespace Wist.Core.Architecture
{
    [InheritedExport]
    public abstract class TypeRegistratorBase
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(TypeRegistratorBase));

        public void RegisterWithResources(IRegistrationManager registrationManager)
        {
            //RegisterResources();

            Register(registrationManager);
        }

        //protected virtual void RegisterResources()
        //{
        //    try
        //    {
        //        Application.Current.Resources.MergedDictionaries.Add(
        //            new ResourceDictionary
        //            {
        //                Source =
        //                    new Uri($@"pack://application:,,,/{GetType().Assembly};component/Resources.xaml")
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        _log.Warn($"Failed to load Resource file for module {GetType().Assembly.FullName}", ex);
        //    }
        //}

        public virtual void Register(IRegistrationManager registrationManager)
        {
            registrationManager.AutoRegisterAssembly(GetType().Assembly);
        }
    }
}

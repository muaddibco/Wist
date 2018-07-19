using CommonServiceLocator;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Unity;
using Wist.Core.Architecture.Enums;
using Wist.Core.Architecture.Registration;
using Wist.Core.Architecture.UnityExtensions;
using Wist.Core.Logging;

namespace Wist.Core.Architecture
{
    public class Bootstrapper
    {
        private IRegistrationManager _registrationManager;

        protected readonly ILog _log;
        protected readonly CancellationToken _cancellationToken;

        public Bootstrapper(CancellationToken ct)
        {
            _log = LogManager.GetLogger(GetType());
            _cancellationToken = ct;
        }

        public IUnityContainer Container { get; private set; }

        public virtual void Run()
        {
            _log.Info("Starting Bootstrap Run");
            try
            {
                Container = CreateContainer();

                ConfigureContainer();

                ConfigureServiceLocator();
            }
            finally
            {
                _log.Info("Bootstrap Run completed");
            }
        }

        protected virtual IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return new string[] { "Wist.Core.dll" };
        }

        #region Private Functions

        private RegistrationSettings GetRegistrationSettings()
        {
            _log.Info("Obtaining Registration Settings started");
            try
            {
                AggregateCatalog coreCatalog = new AggregateCatalog();
                string exeFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string exeName = Path.GetFileName(Assembly.GetEntryAssembly().Location);

                if (exeFolder != null)
                {
                    if (exeName != null) coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, exeName));

                    foreach (string catalogItemName in EnumerateCatalogItems(exeFolder))
                    {
                        coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, catalogItemName));
                    }
                }

                return new RegistrationSettings
                {
                    MefInitialCatalog = coreCatalog,
                    RunMode = (RunMode)Enum.Parse(typeof(RunMode), ConfigurationManager.AppSettings["RunMode"] ?? "Default")
                };
            }
            catch (Exception ex)
            {
                _log.Fatal("Obtaining Registration Settings failed", ex);
                throw;
            }
            finally
            {
                _log.Info("Obtaining Registration Settings completed");
            }
        }

        private void ConfigureServiceLocator()
        {
            _log.Info("ServiceLocator Configuration started");
            try
            {
                _registrationManager.SetupServiceLocator();
            }
            catch (Exception ex)
            {
                _log.Fatal("ServiceLocator Configuration failed", ex);
                throw;
            }
            finally
            {
                _log.Info("ServiceLocator Configuration completed");
            }
        }

        private IUnityContainer CreateContainer()
        {
            _log.Info("Container Creation started");

            try
            {
                return new ExtendedUnityContainer();
            }
            catch (Exception ex)
            {
                _log.Fatal("Container Creation failed", ex);
                throw;
            }
            finally
            {
                _log.Info("Container Creation completed");
            }
        }

        private void ConfigureContainer()
        {
            _log.Info("Container Configuration started");
            try
            {
                //Container.RegisterInstance(typeof(IUnityContainer), Container);

                RegistrationSettings settings = GetRegistrationSettings();

                _registrationManager = new RegistrationManager(settings.RunMode, Container as ExtendedUnityContainer);

                _registrationManager.AutoRegisterUsingMefCatalog(settings.MefInitialCatalog);
                _registrationManager.CommitRegistrationsToContainer();
            }
            catch (ReflectionTypeLoadException ex)
            {
                if (ex.LoaderExceptions != null)
                {
                    foreach (Exception loaderException in ex.LoaderExceptions)
                    {
                        _log.Fatal(loaderException.Message, loaderException);
                    }
                }

                _log.Fatal("Container Configuration failed", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Fatal("Container Configuration failed", ex);
                throw;
            }
            finally
            {
                _log.Info("Container Configuration completed");
            }
        }

        #endregion Private Functions
    }
}

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
using Wist.Core.Configuration;
using Wist.Core.Logging;
using System.Linq;

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

        public UnityContainer Container { get; private set; }

        public virtual void Run(IDictionary<string, string> args = null)
        {
            _log.Info("Starting Bootstrap Run");
            try
            {
                Container = CreateContainer();

                ConfigureContainer();

                ConfigureServiceLocator();

                InitializeConfiguration();

                RunInitializers();
            }
            finally
            {
                _log.Info("Bootstrap Run completed");
            }
        }

        protected virtual void InitializeConfiguration()
        {
            IEnumerable<IConfigurationSection> configurationSections = Container.ResolveAll<IConfigurationSection>();

            foreach (IConfigurationSection configurationSection in configurationSections)
            {
                configurationSection.Initialize();
            }
        }

        protected virtual void RunInitializers()
        {
            _log.Info("Running initializers started");

            try
            {
                IEnumerable<IInitializer> initializers = Container.ResolveAll<IInitializer>();
                IOrderedEnumerable<IInitializer> initializersOrdered = initializers.OrderBy(i => i.Priority);
                foreach (IInitializer item in initializersOrdered)
                {
                    _log.Info($"Running initializer {item.GetType().FullName}");
                    try
                    {
                        item.Initialize();
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Failed to initialize {item.GetType().FullName}", ex);
                    }
                }
            }
            finally
            {
                _log.Info("Run initializers completed");
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

        protected virtual void ConfigureServiceLocator()
        {
            _log.Info("ServiceLocator Configuration started");
            try
            {
                _registrationManager.SetupServiceLocator();

                Container.Resolve<IApplicationContext>().Container = Container;
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

        private UnityContainer CreateContainer()
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

        protected virtual void ConfigureContainer()
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

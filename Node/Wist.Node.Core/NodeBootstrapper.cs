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

namespace Wist.Node.Core
{
    public class NodeBootstrapper
    {
        private IRegistrationManager _registrationManager;

        private readonly ILog _log;
        private readonly CancellationToken _cancellationToken;

        public NodeBootstrapper(CancellationToken ct)
        {
            _log = LogManager.GetLogger(GetType());
            _cancellationToken = ct;
        }

        public IUnityContainer Container { get; private set; }

        public void Run()
        {
            _log.Info("Starting NodeBootstrap Run");
            try
            {
                Container = CreateContainer();

                ConfigureContainer();

                ConfigureServiceLocator();

                //ConfigureLogger();

                StartNode();
            }
            finally
            {
                _log.Info("NodeBootstrap Run completed");
            }
        }

        private void ConfigureLogger()
        {
            ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
            logger.Initialize("Wist");
        }

        private void StartNode()
        {
            _log.Info("Starting Node");
            try
            {
                NodeMain nodeMain = ServiceLocator.Current.GetInstance<NodeMain>();

                nodeMain.Initialize();

                nodeMain.UpdateKnownNodes();

                nodeMain.EnterNodesGroup();

                nodeMain.Start();
            }
            finally
            {
                _log.Info("Starting Node completed");
            }
        }

        #region Private Functions

        private RegistrationSettings GetRegistrationSettings()
        {
            _log.Info("Obtaining Registration Settings started");
            try
            {
                AggregateCatalog coreCatalog = new AggregateCatalog();
                string exeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string exeName = Path.GetFileName(GetType().Assembly.Location);

                if (exeFolder != null)
                {
                    if (exeName != null) coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, exeName));
                    coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, "Wist.Core.dll"));
                    coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, "Chaos.NaCl.dll"));
                    coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, "Wist.Communication.dll"));
                    coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, "Wist.Node.Core.dll"));

                    foreach (string fileName in Directory.EnumerateFiles(exeFolder, "Wist.BlockLattice.*.dll"))
                    {
                        coreCatalog.Catalogs.Add(new DirectoryCatalog(exeFolder, new FileInfo(fileName).Name));
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

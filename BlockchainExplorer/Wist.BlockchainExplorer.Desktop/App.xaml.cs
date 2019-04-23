using CommonServiceLocator;
using Prism.Ioc;
using Prism.Unity;
using Prism.Unity.Ioc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Wist.BlockchainExplorer.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private readonly ExplorerBootstrapper _explorerBootstrapper;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public App()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _explorerBootstrapper = new ExplorerBootstrapper(_cancellationTokenSource.Token);
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            UnityContainerExtension unityContainerExtension = new UnityContainerExtension(_explorerBootstrapper.CreateContainer());

            return unityContainerExtension;
        }

        protected override void ConfigureServiceLocator()
        {
            _explorerBootstrapper.ConfigureServiceLocator();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _explorerBootstrapper.ConfigureContainer();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override void InitializeShell(Window shell)
        {
            base.InitializeShell(shell);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _cancellationTokenSource.Cancel();

            base.OnExit(e);
        }

        protected override Window CreateShell()
        {
            _explorerBootstrapper.Initialize();

            return ServiceLocator.Current.GetInstance<MainWindow>();
        }
    }
}

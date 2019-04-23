using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Wist.Core.Architecture;

namespace Wist.BlockchainExplorer.Desktop
{
    public class ExplorerBootstrapper : Bootstrapper
    {
        public ExplorerBootstrapper(CancellationToken ct) : base(ct)
        {
        }

        protected override IEnumerable<string> EnumerateCatalogItems(string rootFolder)
        {
            return base.EnumerateCatalogItems(rootFolder).Concat(new string[] { "Wist.Crypto.dll", "Wist.Blockchain.Core.dll" });
        }

        public new IUnityContainer CreateContainer()
        {
            Container = base.CreateContainer();

            return Container;
        }

        public new void ConfigureServiceLocator()
        {
            base.ConfigureServiceLocator();
        }

        public new void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        public void Initialize()
        {
            base.InitializeConfiguration();
            base.RunInitializers();
        }
    }
}
